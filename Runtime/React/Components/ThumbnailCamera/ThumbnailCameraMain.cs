using System;
using UnityEngine;
#if UNITY_2018_2_OR_NEWER
using UnityEngine.Rendering;
#endif
using TextureDimension = UnityEngine.Rendering.TextureDimension;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
#endif
using System.IO;
using System.Collections;

[RequireComponent(typeof(Camera))]
public class ThumbnailCamera : MonoBehaviour
#if UNITY_EDITOR
	, IPreprocessBuildWithReport
#endif
{
	[Header("Capture Settings")]
	[SerializeField] private bool encodeAsJPEG = true;
	[SerializeField] private bool faceCameraDirection = true;
	[SerializeField] private int captureWidth = 2048;

	private Camera cam;
	private const string THUMBNAIL_NAME = "DefaultThumbnail.jpg";

#if UNITY_EDITOR
	public int callbackOrder => 0;

	public void OnPreprocessBuild(BuildReport report)
	{
		Debug.Log("[ThumbnailCamera] Starting build process...");
		
		// Get the build path
		string buildPath = report.summary.outputPath;
		Debug.Log($"[ThumbnailCamera] Build path: {buildPath}");

		// For WebGL, we need to save in the build directory
		if (report.summary.platform == BuildTarget.WebGL)
		{
			// Get the directory containing the build
			string buildDirectory = Path.GetDirectoryName(buildPath);
			if (string.IsNullOrEmpty(buildDirectory))
			{
				Debug.LogError("[ThumbnailCamera] Could not determine build directory!");
				return;
			}

			Debug.Log($"[ThumbnailCamera] Build directory: {buildDirectory}");

			// Create StreamingAssets directory in the build
			string streamingAssetsPath = Path.Combine(buildDirectory, "StreamingAssets");
			Debug.Log($"[ThumbnailCamera] StreamingAssets path: {streamingAssetsPath}");

			if (!Directory.Exists(streamingAssetsPath))
			{
				Debug.Log($"[ThumbnailCamera] Creating StreamingAssets directory at: {streamingAssetsPath}");
				Directory.CreateDirectory(streamingAssetsPath);
			}

			// Create a temporary camera for capturing
			GameObject tempCameraObj = new GameObject("TempThumbnailCamera");
			Camera tempCamera = tempCameraObj.AddComponent<Camera>();
			
			try
			{
				// Set up the temporary camera
				tempCamera.clearFlags = CameraClearFlags.Skybox;
				tempCamera.fieldOfView = 90f;
				tempCamera.nearClipPlane = 0.1f;
				tempCamera.farClipPlane = 1000f;
				tempCamera.depth = -1; // Ensure it renders before other cameras
				tempCamera.renderingPath = RenderingPath.Forward;
				tempCamera.allowHDR = false;
				tempCamera.allowMSAA = false;
				tempCamera.stereoTargetEye = StereoTargetEyeMask.None;
				
				// Position the camera at the origin
				tempCameraObj.transform.position = Vector3.zero;
				tempCameraObj.transform.rotation = Quaternion.identity;

				Debug.Log("[ThumbnailCamera] Temporary camera setup complete");

				// Disable all other cameras temporarily
				var allCameras = UnityEngine.Object.FindObjectsOfType<Camera>();
				var originalStates = new bool[allCameras.Length];
				for (int i = 0; i < allCameras.Length; i++)
				{
					if (allCameras[i] != tempCamera)
					{
						originalStates[i] = allCameras[i].enabled;
						allCameras[i].enabled = false;
					}
				}

				Debug.Log("[ThumbnailCamera] Other cameras disabled");

				try
				{
					// Capture the thumbnail
					Debug.Log("[ThumbnailCamera] Starting capture...");
					byte[] imageData = I360Render.Capture(
						width: captureWidth,
						encodeAsJPEG: encodeAsJPEG,
						renderCam: tempCamera,
						faceCameraDirection: faceCameraDirection
					);

					if (imageData != null)
					{
						Debug.Log($"[ThumbnailCamera] Capture successful, image data size: {imageData.Length} bytes");
						string thumbnailPath = Path.Combine(streamingAssetsPath, THUMBNAIL_NAME);
						Debug.Log($"[ThumbnailCamera] Attempting to save thumbnail to: {thumbnailPath}");
						
						try
						{
							// Ensure the directory exists
							Directory.CreateDirectory(Path.GetDirectoryName(thumbnailPath));
							
							// Save the file
							File.WriteAllBytes(thumbnailPath, imageData);
							
							// Verify the file was created
							if (File.Exists(thumbnailPath))
							{
								Debug.Log($"[ThumbnailCamera] Successfully saved thumbnail to: {thumbnailPath}");
								Debug.Log($"[ThumbnailCamera] File size: {new FileInfo(thumbnailPath).Length} bytes");
							}
							else
							{
								Debug.LogError($"[ThumbnailCamera] File was not created at: {thumbnailPath}");
							}
						}
						catch (System.Exception e)
						{
							Debug.LogError($"[ThumbnailCamera] Error saving thumbnail: {e.Message}\n{e.StackTrace}");
						}
					}
					else
					{
						Debug.LogError("[ThumbnailCamera] Failed to capture thumbnail - imageData is null");
					}
				}
				finally
				{
					// Restore original camera states
					for (int i = 0; i < allCameras.Length; i++)
					{
						if (allCameras[i] != tempCamera)
						{
							allCameras[i].enabled = originalStates[i];
						}
					}
					Debug.Log("[ThumbnailCamera] Original camera states restored");
				}
			}
			finally
			{
				// Clean up the temporary camera
				UnityEngine.Object.DestroyImmediate(tempCameraObj);
				Debug.Log("[ThumbnailCamera] Temporary camera cleaned up");
			}
		}
		else
		{
			Debug.LogWarning("[ThumbnailCamera] Thumbnail capture is only supported for WebGL builds.");
		}
	}

	[MenuItem("Spaces SDK/Capture Thumbnail")]
	public static void CaptureThumbnail()
	{
		// Find the thumbnail camera
		ThumbnailCamera thumbnailCam = FindObjectOfType<ThumbnailCamera>();
		if (thumbnailCam == null)
		{
			Debug.LogError("[ThumbnailCamera] No ThumbnailCamera found in scene!");
			return;
		}

		// Ensure StreamingAssets directory exists in Assets folder
		string streamingAssetsPath = Path.Combine(Application.dataPath, "StreamingAssets");
		if (!Directory.Exists(streamingAssetsPath))
		{
			Directory.CreateDirectory(streamingAssetsPath);
		}

		// Use the actual camera component
		Camera cam = thumbnailCam.GetComponent<Camera>();
		if (cam == null)
		{
			Debug.LogError("[ThumbnailCamera] No Camera component found!");
			return;
		}

		try
		{
			// Capture the thumbnail using the actual camera
			byte[] imageData = I360Render.Capture(
				width: thumbnailCam.captureWidth,
				encodeAsJPEG: thumbnailCam.encodeAsJPEG,
				renderCam: cam,
				faceCameraDirection: thumbnailCam.faceCameraDirection
			);

			if (imageData != null)
			{
				string thumbnailPath = Path.Combine(streamingAssetsPath, THUMBNAIL_NAME);
				File.WriteAllBytes(thumbnailPath, imageData);
				Debug.Log($"[ThumbnailCamera] Thumbnail saved to: {thumbnailPath}");
				
				// Refresh the asset database to show the new file
				AssetDatabase.Refresh();
			}
			else
			{
				Debug.LogError("[ThumbnailCamera] Failed to capture thumbnail - imageData is null");
			}
		}
		catch (Exception e)
		{
			Debug.LogError($"[ThumbnailCamera] Error during capture: {e.Message}");
		}
	}
#endif

	private void Awake()
	{
		// Disable the camera component in runtime
		cam = GetComponent<Camera>();
		if (cam != null)
		{
			cam.enabled = false;
		}
	}
}

public static class I360Render
{
	private static Material equirectangularConverter = null;
	private static int paddingX;

	public static byte[] Capture( int width = 1024, bool encodeAsJPEG = true, Camera renderCam = null, bool faceCameraDirection = true )
	{
		return CaptureInternal( width, encodeAsJPEG, renderCam, faceCameraDirection );
	}

#if UNITY_2018_2_OR_NEWER
	public static void CaptureAsync( Action<byte[]> callback, int width = 1024, bool encodeAsJPEG = true, Camera renderCam = null, bool faceCameraDirection = true )
	{
		CaptureInternal( width, encodeAsJPEG, renderCam, faceCameraDirection, callback );
	}
#endif

#if UNITY_2018_2_OR_NEWER
	private static byte[] CaptureInternal( int width = 1024, bool encodeAsJPEG = true, Camera renderCam = null, bool faceCameraDirection = true, Action<byte[]> asyncCallback = null )
#else
	private static byte[] CaptureInternal( int width = 1024, bool encodeAsJPEG = true, Camera renderCam = null, bool faceCameraDirection = true )
#endif

	{
		if( renderCam == null )
		{
			renderCam = Camera.main;
			if( renderCam == null )
			{
				Debug.LogError( "Error: no camera detected" );

#if UNITY_2018_2_OR_NEWER
				if( asyncCallback != null )
					asyncCallback( null );
#endif

				return null;
			}
		}

		RenderTexture camTarget = renderCam.targetTexture;

		if( equirectangularConverter == null )
		{
			equirectangularConverter = new Material( Shader.Find( "Hidden/I360CubemapToEquirectangular" ) );
			paddingX = Shader.PropertyToID( "_PaddingX" );
		}

#if UNITY_2018_2_OR_NEWER
		bool asyncOperationStarted = false;
#endif
		int cubemapSize = Mathf.Min( Mathf.NextPowerOfTwo( width ), 8192 );
		RenderTexture activeRT = RenderTexture.active;
		RenderTexture cubemap = null, equirectangularTexture = null;
		Texture2D output = null;
		try
		{
			cubemap = RenderTexture.GetTemporary( cubemapSize, cubemapSize, 0 );
			cubemap.dimension = TextureDimension.Cube;
			cubemap.antiAliasing = 1; // Disable MSAA for the cubemap

			equirectangularTexture = RenderTexture.GetTemporary( cubemapSize, cubemapSize / 2, 0 );
			equirectangularTexture.dimension = TextureDimension.Tex2D;
			equirectangularTexture.antiAliasing = 1; // Disable MSAA for the equirectangular texture

			if( !renderCam.RenderToCubemap( cubemap, 63 ) )
			{
				Debug.LogError( "Rendering to cubemap is not supported on device/platform!" );

#if UNITY_2018_2_OR_NEWER
				if( asyncCallback != null )
					asyncCallback( null );
#endif

				return null;
			}

			equirectangularConverter.SetFloat( paddingX, faceCameraDirection ? ( renderCam.transform.eulerAngles.y / 360f ) : 0f );
			Graphics.Blit( cubemap, equirectangularTexture, equirectangularConverter );

#if UNITY_2018_2_OR_NEWER
			if( asyncCallback != null )
			{
				AsyncGPUReadback.Request( equirectangularTexture, 0, TextureFormat.RGB24, ( asyncResult ) =>
				{
					try
					{
						output = new Texture2D( equirectangularTexture.width, equirectangularTexture.height, TextureFormat.RGB24, false );
						if( !asyncResult.hasError )
							output.LoadRawTextureData( asyncResult.GetData<byte>() );
						else
						{
							Debug.LogError( "Async thumbnail request failed, falling back to conventional method" );

							RenderTexture _activeRT = RenderTexture.active;
							try
							{
								RenderTexture.active = equirectangularTexture;
								output.ReadPixels( new Rect( 0, 0, equirectangularTexture.width, equirectangularTexture.height ), 0, 0 );
							}
							finally
							{
								RenderTexture.active = _activeRT;
							}
						}

						asyncCallback( encodeAsJPEG ? InsertXMPIntoTexture2D_JPEG( output ) : InsertXMPIntoTexture2D_PNG( output ) );
					}
					finally
					{
						if( equirectangularTexture )
							RenderTexture.ReleaseTemporary( equirectangularTexture );

						if( output )
							UnityEngine.Object.DestroyImmediate( output );
					}
				} );

				asyncOperationStarted = true;
				return null;
			}
			else
#endif
			{
				RenderTexture.active = equirectangularTexture;
				output = new Texture2D( equirectangularTexture.width, equirectangularTexture.height, TextureFormat.RGB24, false );
				output.ReadPixels( new Rect( 0, 0, equirectangularTexture.width, equirectangularTexture.height ), 0, 0 );
				return encodeAsJPEG ? output.EncodeToJPG(100) : output.EncodeToPNG();
			}
		}
		catch( Exception e )
		{
			Debug.LogException( e );

#if UNITY_2018_2_OR_NEWER
			if( !asyncOperationStarted && asyncCallback != null )
				asyncCallback( null );
#endif

			return null;
		}
		finally
		{
			renderCam.targetTexture = camTarget;

#if UNITY_2018_2_OR_NEWER
			if( !asyncOperationStarted )
#endif
			{
				RenderTexture.active = activeRT;
			}

			if( cubemap )
				RenderTexture.ReleaseTemporary( cubemap );

			if( equirectangularTexture )
			{
#if UNITY_2018_2_OR_NEWER
				if( !asyncOperationStarted )
#endif
				{
					RenderTexture.ReleaseTemporary( equirectangularTexture );
				}
			}

			if( output )
			{
#if UNITY_2018_2_OR_NEWER
				if( !asyncOperationStarted )
#endif
				{
					UnityEngine.Object.DestroyImmediate( output );
				}
			}
		}
	}

	#region XMP Injection
	private const string XMP_NAMESPACE_JPEG = "http://ns.adobe.com/xap/1.0/";
	private const string XMP_CONTENT_TO_FORMAT_JPEG = "<x:xmpmeta xmlns:x=\"adobe:ns:meta/\" x:xmptk=\"Adobe XMP Core 5.1.0-jc003\"> <rdf:RDF xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\"> <rdf:Description rdf:about=\"\" xmlns:GPano=\"http://ns.google.com/photos/1.0/panorama/\" GPano:UsePanoramaViewer=\"True\" GPano:CaptureSoftware=\"Unity3D\" GPano:StitchingSoftware=\"Unity3D\" GPano:ProjectionType=\"equirectangular\" GPano:PoseHeadingDegrees=\"180.0\" GPano:InitialViewHeadingDegrees=\"0.0\" GPano:InitialViewPitchDegrees=\"0.0\" GPano:InitialViewRollDegrees=\"0.0\" GPano:InitialHorizontalFOVDegrees=\"{0}\" GPano:CroppedAreaLeftPixels=\"0\" GPano:CroppedAreaTopPixels=\"0\" GPano:CroppedAreaImageWidthPixels=\"{1}\" GPano:CroppedAreaImageHeightPixels=\"{2}\" GPano:FullPanoWidthPixels=\"{1}\" GPano:FullPanoHeightPixels=\"{2}\"/></rdf:RDF></x:xmpmeta>";
	private const string XMP_CONTENT_TO_FORMAT_PNG = "XML:com.adobe.xmp\0\0\0\0\0<?xpacket begin=\"ï»¿\" id=\"W5M0MpCehiHzreSzNTczkc9d\"?><x:xmpmeta xmlns:x=\"adobe:ns:meta/\" x:xmptk=\"Adobe XMP Core 5.1.0-jc003\"> <rdf:RDF xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\"> <rdf:Description rdf:about=\"\" xmlns:GPano=\"http://ns.google.com/photos/1.0/panorama/\" xmlns:xmp=\"http://ns.adobe.com/xap/1.0/\" xmlns:dc=\"http://purl.org/dc/elements/1.1/\" xmlns:xmpMM=\"http://ns.adobe.com/xap/1.0/mm/\" xmlns:stEvt=\"http://ns.adobe.com/xap/1.0/sType/ResourceEvent#\" xmlns:tiff=\"http://ns.adobe.com/tiff/1.0/\" xmlns:exif=\"http://ns.adobe.com/exif/1.0/\"> <GPano:UsePanoramaViewer>True</GPano:UsePanoramaViewer> <GPano:CaptureSoftware>Unity3D</GPano:CaptureSoftware> <GPano:StitchingSoftware>Unity3D</GPano:StitchingSoftware> <GPano:ProjectionType>equirectangular</GPano:ProjectionType> <GPano:PoseHeadingDegrees>180.0</GPano:PoseHeadingDegrees> <GPano:InitialViewHeadingDegrees>0.0</GPano:InitialViewHeadingDegrees> <GPano:InitialViewPitchDegrees>0.0</GPano:InitialViewPitchDegrees> <GPano:InitialViewRollDegrees>0.0</GPano:InitialViewRollDegrees> <GPano:InitialHorizontalFOVDegrees>{0}</GPano:InitialHorizontalFOVDegrees> <GPano:CroppedAreaLeftPixels>0</GPano:CroppedAreaLeftPixels> <GPano:CroppedAreaTopPixels>0</GPano:CroppedAreaTopPixels> <GPano:CroppedAreaImageWidthPixels>{1}</GPano:CroppedAreaImageWidthPixels> <GPano:CroppedAreaImageHeightPixels>{2}</GPano:CroppedAreaImageHeightPixels> <GPano:FullPanoWidthPixels>{1}</GPano:FullPanoWidthPixels> <GPano:FullPanoHeightPixels>{2}</GPano:FullPanoHeightPixels> <tiff:Orientation>1</tiff:Orientation> <exif:PixelXDimension>{1}</exif:PixelXDimension> <exif:PixelYDimension>{2}</exif:PixelYDimension> </rdf:Description></rdf:RDF></x:xmpmeta><?xpacket end=\"w\"?>";
	private static uint[] CRC_TABLE_PNG = null;

	public static byte[] InsertXMPIntoTexture2D_JPEG( Texture2D image )
	{
		return DoTheHardWork_JPEG( image.EncodeToJPG( 100 ), image.width, image.height );
	}

	public static byte[] InsertXMPIntoTexture2D_PNG( Texture2D image )
	{
		return DoTheHardWork_PNG( image.EncodeToPNG(), image.width, image.height );
	}

	#region JPEG Encoding
	private static byte[] DoTheHardWork_JPEG( byte[] fileBytes, int imageWidth, int imageHeight )
	{
		int xmpIndex = 0, xmpContentSize = 0;
		while( !SearchChunkForXMP_JPEG( fileBytes, ref xmpIndex, ref xmpContentSize ) )
		{
			if( xmpIndex == -1 )
				break;
		}

		int copyBytesUntil, copyBytesFrom;
		if( xmpIndex == -1 )
		{
			copyBytesUntil = copyBytesFrom = FindIndexToInsertXMPCode_JPEG( fileBytes );
		}
		else
		{
			copyBytesUntil = xmpIndex;
			copyBytesFrom = xmpIndex + 2 + xmpContentSize;
		}

		string xmpContent = string.Concat( XMP_NAMESPACE_JPEG, "\0", string.Format( XMP_CONTENT_TO_FORMAT_JPEG, 75f.ToString( "F1" ), imageWidth, imageHeight ) );
		int xmpLength = xmpContent.Length + 2;
		xmpContent = string.Concat( (char) 0xFF, (char) 0xE1, (char) ( xmpLength / 256 ), (char) ( xmpLength % 256 ), xmpContent );

		byte[] result = new byte[copyBytesUntil + xmpContent.Length + ( fileBytes.Length - copyBytesFrom )];

		Array.Copy( fileBytes, 0, result, 0, copyBytesUntil );

		for( int i = 0; i < xmpContent.Length; i++ )
		{
			result[copyBytesUntil + i] = (byte) xmpContent[i];
		}

		Array.Copy( fileBytes, copyBytesFrom, result, copyBytesUntil + xmpContent.Length, fileBytes.Length - copyBytesFrom );

		return result;
	}

	private static bool CheckBytesForXMPNamespace_JPEG( byte[] bytes, int startIndex )
	{
		for( int i = 0; i < XMP_NAMESPACE_JPEG.Length; i++ )
		{
			if( bytes[startIndex + i] != XMP_NAMESPACE_JPEG[i] )
				return false;
		}

		return true;
	}

	private static bool SearchChunkForXMP_JPEG( byte[] bytes, ref int startIndex, ref int chunkSize )
	{
		if( startIndex + 4 < bytes.Length )
		{
			//Debug.Log( startIndex + " " + System.Convert.ToByte( bytes[startIndex] ).ToString( "x2" ) + " " + System.Convert.ToByte( bytes[startIndex+1] ).ToString( "x2" ) + " " +
			//           System.Convert.ToByte( bytes[startIndex+2] ).ToString( "x2" ) + " " + System.Convert.ToByte( bytes[startIndex+3] ).ToString( "x2" ) );

			if( bytes[startIndex] == 0xFF )
			{
				byte secondByte = bytes[startIndex + 1];
				if( secondByte == 0xDA )
				{
					startIndex = -1;
					return false;
				}
				else if( secondByte == 0x01 || ( secondByte >= 0xD0 && secondByte <= 0xD9 ) )
				{
					startIndex += 2;
					return false;
				}
				else
				{
					chunkSize = bytes[startIndex + 2] * 256 + bytes[startIndex + 3];

					if( secondByte == 0xE1 && chunkSize >= 31 && CheckBytesForXMPNamespace_JPEG( bytes, startIndex + 4 ) )
					{
						return true;
					}

					startIndex = startIndex + 2 + chunkSize;
				}
			}
		}

		return false;
	}

	private static int FindIndexToInsertXMPCode_JPEG( byte[] bytes )
	{
		int chunkSize = bytes[4] * 256 + bytes[5];
		return chunkSize + 4;
	}
	#endregion

	#region PNG Encoding
	private static byte[] DoTheHardWork_PNG( byte[] fileBytes, int imageWidth, int imageHeight )
	{
		string xmpContent = "iTXt" + string.Format( XMP_CONTENT_TO_FORMAT_PNG, 75f.ToString( "F1" ), imageWidth, imageHeight );
		int copyBytesUntil = 33;
		int xmpLength = xmpContent.Length - 4; // minus iTXt
		string xmpCRC = CalculateCRC_PNG( xmpContent );
		xmpContent = string.Concat( (char) ( xmpLength >> 24 ), (char) ( xmpLength >> 16 ), (char) ( xmpLength >> 8 ), (char) ( xmpLength ),
									xmpContent, xmpCRC );

		byte[] result = new byte[fileBytes.Length + xmpContent.Length];

		Array.Copy( fileBytes, 0, result, 0, copyBytesUntil );

		for( int i = 0; i < xmpContent.Length; i++ )
		{
			result[copyBytesUntil + i] = (byte) xmpContent[i];
		}

		Array.Copy( fileBytes, copyBytesUntil, result, copyBytesUntil + xmpContent.Length, fileBytes.Length - copyBytesUntil );

		return result;
	}

	// Source: https://github.com/damieng/DamienGKit/blob/master/CSharp/DamienG.Library/Security/Cryptography/Crc32.cs
	private static string CalculateCRC_PNG( string xmpContent )
	{
		if( CRC_TABLE_PNG == null )
			CalculateCRCTable_PNG();

		uint crc = ~UpdateCRC_PNG( xmpContent );
		byte[] crcBytes = CalculateCRCBytes_PNG( crc );

		return string.Concat( (char) crcBytes[0], (char) crcBytes[1], (char) crcBytes[2], (char) crcBytes[3] );
	}

	private static uint UpdateCRC_PNG( string xmpContent )
	{
		uint c = 0xFFFFFFFF;
		for( int i = 0; i < xmpContent.Length; i++ )
		{
			c = ( c >> 8 ) ^ CRC_TABLE_PNG[xmpContent[i] ^ c & 0xFF];
		}

		return c;
	}

	private static void CalculateCRCTable_PNG()
	{
		CRC_TABLE_PNG = new uint[256];
		for( uint i = 0; i < 256; i++ )
		{
			uint c = i;
			for( int j = 0; j < 8; j++ )
			{
				if( ( c & 1 ) == 1 )
					c = ( c >> 1 ) ^ 0xEDB88320;
				else
					c = ( c >> 1 );
			}

			CRC_TABLE_PNG[i] = c;
		}
	}

	private static byte[] CalculateCRCBytes_PNG( uint crc )
	{
		var result = BitConverter.GetBytes( crc );

		if( BitConverter.IsLittleEndian )
			Array.Reverse( result );

		return result;
	}
	#endregion
	#endregion
}
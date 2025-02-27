using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Spaces.React.Runtime;

public class ThumbnailReceiver : MonoBehaviour
{
    [Tooltip("The target mesh where the thumbnail will be rendered.")]
    public Transform targetMesh;

    private string objectName;

    private void Awake()
    {
        // Automatically use the GameObject's name as the identifier
        objectName = gameObject.name;
    }

    private void OnEnable()
    {
        // Subscribe to the OnReactSetThumbnail event
        ReactIncomingEvent.OnReactSetThumbnail += HandleSetThumbnail;
    }

    private void OnDisable()
    {
        // Unsubscribe from the OnReactSetThumbnail event
        ReactIncomingEvent.OnReactSetThumbnail -= HandleSetThumbnail;
    }

    private void HandleSetThumbnail(ThumbnailData data)
    {
        // Check if the incoming data matches this object's name
        if (data.gameObjectName == objectName)
        {
            Debug.Log($"Setting thumbnail for {objectName} with URL: {data.thumbnailUrl}");
            
            // Start loading the thumbnail as a texture
            StartCoroutine(LoadThumbnail(data.thumbnailUrl));
        }
    }

    private IEnumerator LoadThumbnail(string thumbnailUrl)
    {
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(thumbnailUrl))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(uwr);

                // Get the MeshRenderer from the target mesh
                MeshRenderer renderer = (targetMesh != null) ? targetMesh.GetComponent<MeshRenderer>() : GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    if (renderer.material == null)
                        renderer.material = new Material(Shader.Find("Standard"));
                    renderer.material.mainTexture = texture;
                }
                else
                {
                    Debug.LogError($"No MeshRenderer found on target mesh '{(targetMesh != null ? targetMesh.name : name)}'.");
                }
            }
            else
            {
                Debug.LogError($"Failed to download thumbnail: {uwr.error}");
            }
        }
    }
}

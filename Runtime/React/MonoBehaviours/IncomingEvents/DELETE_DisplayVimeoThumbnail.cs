using UnityEngine;
using System.Collections;
using Spaces.React.Runtime;

public class DisplayVimeoThumbnail : MonoBehaviour
{
    public Renderer targetRenderer; // The Renderer to display the thumbnail on the game object

    private void OnEnable()
    {
        // Subscribe to the event from ReactIncomingEvent
        ReactIncomingEvent.OnReactDisplayVimeoThumbnail += HandleDisplayVimeoThumbnail;
    }

    private void OnDisable()
    {
        // Unsubscribe from the event to avoid memory leaks
        ReactIncomingEvent.OnReactDisplayVimeoThumbnail -= HandleDisplayVimeoThumbnail;
    }

    private void HandleDisplayVimeoThumbnail(VimeoThumbnailData data)
    {
        if (data != null && !string.IsNullOrEmpty(data.thumbnailUrl))
        {
            Debug.Log("Received thumbnail URL: " + data.thumbnailUrl);

            // Check if this script's game object matches the one sent from React
            if (data.gameObjectName == gameObject.name) // Match with the current game object
            {
                // Start a coroutine to download and apply the thumbnail
                StartCoroutine(DownloadAndApplyThumbnail(data.thumbnailUrl));
            }
        }
        else
        {
            Debug.LogWarning("Received null or empty thumbnail data.");
        }
    }

    private IEnumerator DownloadAndApplyThumbnail(string url)
    {
        using (UnityEngine.Networking.UnityWebRequest request = UnityEngine.Networking.UnityWebRequestTexture.GetTexture(url))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error downloading thumbnail: " + request.error);
            }
            else
            {
                // Get the downloaded texture
                Texture2D texture = UnityEngine.Networking.DownloadHandlerTexture.GetContent(request);

                // Apply the texture to the target Renderer
                if (targetRenderer != null)
                {
                    targetRenderer.material.mainTexture = texture;
                    Debug.Log("Thumbnail applied successfully to the target renderer.");
                }
                else
                {
                    Debug.LogWarning("No target Renderer assigned to DisplayVimeoThumbnail script.");
                }
            }
        }
    }
}

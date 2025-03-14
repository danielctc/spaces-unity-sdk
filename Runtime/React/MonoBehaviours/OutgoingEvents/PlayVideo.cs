using UnityEngine;
using Spaces.React.Runtime;

public class PlayVideo : MonoBehaviour
{
    private void OnEnable()
    {
        // Removed subscription to OnToggleEditMode
    }

    private void OnDisable()
    {
        // Removed unsubscription from OnToggleEditMode
    }

    private void OnMouseDown()
    {
        // Removed edit mode conditional logic
        Debug.Log("Unity: Clicked to play video for game object - name: " + gameObject.name);
        PlayVideoData playVideoData = new PlayVideoData
        {
            gameObjectName = gameObject.name
        };
        ReactRaiseEvent.PlayVideo(playVideoData);
    }
}

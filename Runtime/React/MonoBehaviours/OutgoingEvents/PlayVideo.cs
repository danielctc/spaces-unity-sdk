using UnityEngine;
using Spaces.React.Runtime;

public class PlayVideo : MonoBehaviour
{
    private bool isEditMode;

    private void OnEnable()
    {
        ReactIncomingEvent.OnToggleEditMode += HandleEditModeToggle;
    }

    private void OnDisable()
    {
        ReactIncomingEvent.OnToggleEditMode -= HandleEditModeToggle;
    }

    private void HandleEditModeToggle(bool editMode)
    {
        isEditMode = editMode;
    }

    private void OnMouseDown()
    {
        if (isEditMode)
        {
            Debug.Log("Unity: Edit mode active, opening edit modal for game object - name: " + gameObject.name);
            EditModeData editModeData = new EditModeData
            {
                gameObjectName = gameObject.name
            };
            ReactRaiseEvent.OpenEditModal(editModeData);
        }
        else
        {
            Debug.Log("Unity: Clicked to play video for game object - name: " + gameObject.name);
            PlayVideoData playVideoData = new PlayVideoData
            {
                gameObjectName = gameObject.name
            };
            ReactRaiseEvent.PlayVideo(playVideoData);
        }
    }
}

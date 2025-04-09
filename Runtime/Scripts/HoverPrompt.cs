using UnityEngine;
using UnityEngine.EventSystems; // Required for hover detection interfaces
using TMPro; // Required for TextMeshPro

namespace Spaces.Fusion.Runtime // Added namespace
{
    public class HoverPrompt : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Prompt Content")]
        [Tooltip("The text message to display on hover.")]
        [SerializeField] private string promptMessage = "World Space Prompt";

        [Header("References")]
        [Tooltip("Assign the parent GameObject holding the World Space Canvas and Text element.")]
        [SerializeField] private GameObject promptCanvasHolder;

        private TextMeshProUGUI promptTextComponent;
        private bool isHovering = false; // Track hover state

        void Start()
        {
            if (promptCanvasHolder != null)
            {
                // Attempt to find the TextMeshProUGUI component within the holder's children
                promptTextComponent = promptCanvasHolder.GetComponentInChildren<TextMeshProUGUI>(true); // Include inactive children

                if (promptTextComponent == null)
                {
                    Debug.LogError($"HoverPrompt: No TextMeshProUGUI component found on children of '{promptCanvasHolder.name}'. Please ensure one exists.", this);
                }

                // Ensure the prompt starts hidden
                promptCanvasHolder.SetActive(false);
            }
            else
            {
                Debug.LogError("HoverPrompt: 'Prompt Canvas Holder' is not assigned in the Inspector!", this);
            }
        }

        // Called by the EventSystem when the mouse pointer enters the object's Collider bounds
        public void OnPointerEnter(PointerEventData eventData)
        {
            isHovering = true;
            ShowPrompt();
        }

        // Called by the EventSystem when the mouse pointer exits the object's Collider bounds
        public void OnPointerExit(PointerEventData eventData)
        {
            isHovering = false;
            HidePrompt();
        }

        // Also hide the prompt if this object gets disabled while hovering
        void OnDisable()
        {
            if (isHovering)
            {
               HidePrompt();
               isHovering = false; // Reset hover state
            }
             // Ensure prompt is hidden if object is disabled for other reasons too
             else if (promptCanvasHolder != null && promptCanvasHolder.activeSelf)
             {
                 HidePrompt();
             }
        }

        private void ShowPrompt()
        {
            if (promptCanvasHolder == null || promptTextComponent == null) return;

            // Update the text content
            promptTextComponent.text = promptMessage;
            // Activate the holder GameObject (which contains the canvas and text)
            promptCanvasHolder.SetActive(true);
        }

        private void HidePrompt()
        {
            if (promptCanvasHolder != null)
            {
                // Deactivate the holder GameObject
                promptCanvasHolder.SetActive(false);
            }
        }

         // Optional: Allow updating the prompt message dynamically from other scripts
        public void SetPromptMessage(string newMessage)
        {
            promptMessage = newMessage;
            // If currently hovering, update the displayed text immediately
            if (isHovering && promptTextComponent != null)
            {
                promptTextComponent.text = promptMessage;
            }
        }
    }
}
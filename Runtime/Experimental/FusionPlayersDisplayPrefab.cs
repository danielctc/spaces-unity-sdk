using UnityEngine;

namespace Spaces.Core.Experimental
{
    /// <summary>
    /// Helper component for setting up a FusionPlayersDisplay prefab.
    /// This script can be attached to a prefab with the FusionPlayersDisplay component
    /// to ensure proper initialization when instantiated at runtime.
    /// </summary>
    public class FusionPlayersDisplayPrefab : MonoBehaviour
    {
        [SerializeField] private bool autoSetupCanvas = true;
        [SerializeField] private bool dontDestroyOnLoad = true;
        
        private void Awake()
        {
            if (dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }
            
            if (autoSetupCanvas && GetComponent<Canvas>() == null)
            {
                SetupCanvas();
            }
        }
        
        private void SetupCanvas()
        {
            // Add canvas and required components
            Canvas canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100; // Make sure it's visible on top of other UIs
            
            // Add canvas scaler for proper scaling across different resolutions
            UnityEngine.UI.CanvasScaler scaler = gameObject.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f; // Balance width/height scaling
            
            // Add raycaster for interactivity
            gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            
            // Set default position and size
            RectTransform rect = GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = new Vector2(0.8f, 0.6f);
                rect.anchorMax = new Vector2(1.0f, 1.0f);
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
            }
            
            // Add background panel
            GameObject panel = new GameObject("Background");
            panel.transform.SetParent(transform, false);
            
            RectTransform panelRect = panel.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            
            UnityEngine.UI.Image panelImage = panel.AddComponent<UnityEngine.UI.Image>();
            panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
            
            // Make sure panel is behind other UI elements
            panel.transform.SetAsFirstSibling();
            
            Debug.Log("[FusionPlayersDisplayPrefab] Canvas setup complete");
        }

        /// <summary>
        /// Helper method to create a complete FusionPlayersDisplay gameObject at runtime
        /// </summary>
        public static GameObject CreatePlayersDisplayInstance()
        {
            // Create root object
            GameObject displayObj = new GameObject("FusionPlayersDisplay");
            
            // Add required components
            displayObj.AddComponent<FusionPlayersDisplayPrefab>();
            displayObj.AddComponent<FusionPlayersDisplay>();
            
            return displayObj;
        }
    }
} 
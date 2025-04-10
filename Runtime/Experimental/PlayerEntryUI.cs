using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using Fusion;

namespace Spaces.Core.Experimental
{
    /// <summary>
    /// UI component for a player entry in the Fusion Players Display.
    /// This script handles the display of player information in the UI.
    /// </summary>
    public class PlayerEntryUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI playerNameText;
        [SerializeField] private TextMeshProUGUI playerIdText;
        [SerializeField] private Image playerIcon;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Button kickButton;
        [SerializeField] private Color localPlayerColor = new Color(0.2f, 0.7f, 1f, 0.3f);
        [SerializeField] private Color remotePlayerColor = new Color(0.2f, 0.2f, 0.2f, 0.2f);

        private PlayerRef playerRef;
        
        public void SetPlayerInfo(string playerName, string playerId, bool isLocalPlayer, PlayerRef playerReference)
        {
            playerRef = playerReference;
            
            if (playerNameText != null)
            {
                playerNameText.text = playerName;
            }

            if (playerIdText != null)
            {
                playerIdText.text = $"ID: {playerId}";
            }

            if (backgroundImage != null)
            {
                backgroundImage.color = isLocalPlayer ? localPlayerColor : remotePlayerColor;
            }

            // Add local player indicator
            if (isLocalPlayer && playerNameText != null)
            {
                playerNameText.text = $"➤ {playerNameText.text}";
                
                // Hide kick button for local player
                if (kickButton != null)
                {
                    kickButton.gameObject.SetActive(false);
                }
            }
            else if (kickButton != null)
            {
                kickButton.gameObject.SetActive(true);
            }
        }

        public void SetKickButtonCallback(UnityAction callback)
        {
            if (kickButton != null)
            {
                kickButton.onClick.RemoveAllListeners();
                kickButton.onClick.AddListener(callback);
            }
        }

        public PlayerRef GetPlayerRef()
        {
            return playerRef;
        }

        // Helper method to create a new player entry GameObject
        public static GameObject CreatePlayerEntryGameObject(Transform parent)
        {
            // Create main GameObject
            GameObject entryGO = new GameObject("PlayerEntry", typeof(RectTransform), typeof(PlayerEntryUI));
            entryGO.transform.SetParent(parent, false);
            
            RectTransform rect = entryGO.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(0.5f, 1);
            rect.sizeDelta = new Vector2(0, 40);

            // Add background
            GameObject bgGO = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bgGO.transform.SetParent(entryGO.transform, false);
            
            RectTransform bgRect = bgGO.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            
            Image bgImage = bgGO.GetComponent<Image>();
            bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.2f);

            // Add player name text
            GameObject nameGO = new GameObject("PlayerName", typeof(RectTransform), typeof(TextMeshProUGUI));
            nameGO.transform.SetParent(entryGO.transform, false);
            
            RectTransform nameRect = nameGO.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0, 0.5f);
            nameRect.anchorMax = new Vector2(0.5f, 0.5f);
            nameRect.pivot = new Vector2(0, 0.5f);
            nameRect.offsetMin = new Vector2(10, -15);
            nameRect.offsetMax = new Vector2(0, 15);
            
            TextMeshProUGUI nameText = nameGO.GetComponent<TextMeshProUGUI>();
            nameText.fontSize = 14;
            nameText.color = Color.white;
            nameText.alignment = TextAlignmentOptions.Left;
            
            // Add player ID text
            GameObject idGO = new GameObject("PlayerId", typeof(RectTransform), typeof(TextMeshProUGUI));
            idGO.transform.SetParent(entryGO.transform, false);
            
            RectTransform idRect = idGO.GetComponent<RectTransform>();
            idRect.anchorMin = new Vector2(0.5f, 0.5f);
            idRect.anchorMax = new Vector2(0.7f, 0.5f);
            idRect.pivot = new Vector2(0.5f, 0.5f);
            idRect.offsetMin = new Vector2(0, -15);
            idRect.offsetMax = new Vector2(0, 15);
            
            TextMeshProUGUI idText = idGO.GetComponent<TextMeshProUGUI>();
            idText.fontSize = 12;
            idText.color = new Color(0.7f, 0.7f, 0.7f);
            idText.alignment = TextAlignmentOptions.Center;
            
            // Add kick button
            GameObject kickBtnGO = new GameObject("KickButton", typeof(RectTransform), typeof(Image), typeof(Button));
            kickBtnGO.transform.SetParent(entryGO.transform, false);
            
            RectTransform kickRect = kickBtnGO.GetComponent<RectTransform>();
            kickRect.anchorMin = new Vector2(0.7f, 0.5f);
            kickRect.anchorMax = new Vector2(0.95f, 0.5f);
            kickRect.pivot = new Vector2(0.5f, 0.5f);
            kickRect.offsetMin = new Vector2(0, -15);
            kickRect.offsetMax = new Vector2(0, 15);
            
            Image kickImage = kickBtnGO.GetComponent<Image>();
            kickImage.color = new Color(0.8f, 0.2f, 0.2f, 0.7f);
            
            Button kickButton = kickBtnGO.GetComponent<Button>();
            
            // Add text to the button
            GameObject kickTextGO = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            kickTextGO.transform.SetParent(kickBtnGO.transform, false);
            
            RectTransform kickTextRect = kickTextGO.GetComponent<RectTransform>();
            kickTextRect.anchorMin = Vector2.zero;
            kickTextRect.anchorMax = Vector2.one;
            kickTextRect.offsetMin = Vector2.zero;
            kickTextRect.offsetMax = Vector2.zero;
            
            TextMeshProUGUI kickText = kickTextGO.GetComponent<TextMeshProUGUI>();
            kickText.text = "Kick";
            kickText.fontSize = 12;
            kickText.color = Color.white;
            kickText.alignment = TextAlignmentOptions.Center;

            // Set up references
            PlayerEntryUI entryUI = entryGO.GetComponent<PlayerEntryUI>();
            entryUI.playerNameText = nameText;
            entryUI.playerIdText = idText;
            entryUI.backgroundImage = bgImage;
            entryUI.kickButton = kickButton;

            return entryGO;
        }
    }
} 
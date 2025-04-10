using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using NinjutsuGames.FusionNetwork.Runtime;
using System.Linq;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

namespace Spaces.Core.Experimental
{
    /// <summary>
    /// Test component that displays all players currently in a Fusion Shared Mode room.
    /// Attach this to a GameObject with a Canvas and it will create a UI display showing all connected players.
    /// </summary>
    public class FusionPlayersDisplay : NetworkBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Transform contentParent;
        [SerializeField] private TextMeshProUGUI playerCountText;
        [SerializeField] private GameObject playerEntryPrefab;
        [SerializeField] private float refreshInterval = 1.0f;
        
        [Header("Kick UI")]
        [SerializeField] private GameObject kickConfirmationPanel;
        [SerializeField] private TextMeshProUGUI kickConfirmationText;
        [SerializeField] private Button confirmKickButton;
        [SerializeField] private Button cancelKickButton;
        [SerializeField] private GameObject kickResultPanel;
        [SerializeField] private TextMeshProUGUI kickResultText;
        [SerializeField] private float resultDisplayTime = 2f;

        [Header("Debug Settings")]
        [SerializeField] private bool logPlayersOnRefresh = false;
        [SerializeField] private bool showDetailedPlayerInfo = true;

        private Dictionary<PlayerRef, GameObject> playerEntries = new Dictionary<PlayerRef, GameObject>();
        private float refreshTimer = 0f;
        private NetworkRunner runner;
        private PlayerRef playerToKick;
        private bool isKickPanelOpen = false;
        private GameObject _blockingBackground;

        private void Awake()
        {
            // Create UI container if not assigned
            CreateUIElements();
            
            // Initialize kick UI if not assigned
            InitializeKickUI();
        }

        private void OnEnable()
        {
            // Ensure our kick UI is properly set up when the component is enabled
            if (confirmKickButton != null && cancelKickButton != null)
            {
                // Re-add the button listeners in case they got lost
                confirmKickButton.onClick.RemoveAllListeners();
                confirmKickButton.onClick.AddListener(ConfirmKickPlayer);
                
                cancelKickButton.onClick.RemoveAllListeners();
                cancelKickButton.onClick.AddListener(CancelKickPlayer);
            }
        }

        private void CreateUIElements()
        {
            if (GetComponent<RectTransform>() == null)
            {
                gameObject.AddComponent<RectTransform>();
            }

            // Create content parent if not assigned
            if (contentParent == null)
            {
                GameObject contentGO = new GameObject("PlayersContent", typeof(RectTransform));
                contentGO.transform.SetParent(transform, false);
                contentParent = contentGO.transform;
                
                RectTransform rect = contentGO.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0, 0);
                rect.anchorMax = new Vector2(1, 1);
                rect.offsetMin = new Vector2(10, 10);
                rect.offsetMax = new Vector2(-10, -50);

                // Add scroll view for many players
                ScrollRect scrollRect = contentGO.AddComponent<ScrollRect>();
                
                // Create scroll content
                GameObject scrollContent = new GameObject("Content", typeof(RectTransform));
                scrollContent.transform.SetParent(contentGO.transform, false);
                
                RectTransform contentRect = scrollContent.GetComponent<RectTransform>();
                contentRect.anchorMin = new Vector2(0, 1);
                contentRect.anchorMax = new Vector2(1, 1);
                contentRect.pivot = new Vector2(0.5f, 1);
                contentRect.sizeDelta = new Vector2(0, 0);
                
                // Setup scroll rect
                scrollRect.content = contentRect;
                scrollRect.horizontal = false;
                scrollRect.vertical = true;
                scrollRect.scrollSensitivity = 20;
                scrollRect.viewport = rect;
                
                // Update content parent reference
                contentParent = scrollContent.transform;
                
                // Add vertical layout group
                VerticalLayoutGroup layoutGroup = scrollContent.AddComponent<VerticalLayoutGroup>();
                layoutGroup.childAlignment = TextAnchor.UpperCenter;
                layoutGroup.spacing = 5;
                layoutGroup.padding = new RectOffset(5, 5, 5, 5);
                layoutGroup.childForceExpandWidth = true;
                layoutGroup.childForceExpandHeight = false;
                layoutGroup.childControlWidth = true;
                layoutGroup.childControlHeight = false;
                
                // Add content size fitter
                ContentSizeFitter sizeFitter = scrollContent.AddComponent<ContentSizeFitter>();
                sizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            }

            // Create player count text if not assigned
            if (playerCountText == null)
            {
                GameObject countGO = new GameObject("PlayerCount", typeof(RectTransform), typeof(TextMeshProUGUI));
                countGO.transform.SetParent(transform, false);
                
                RectTransform rect = countGO.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0, 1);
                rect.anchorMax = new Vector2(1, 1);
                rect.offsetMin = new Vector2(10, -40);
                rect.offsetMax = new Vector2(-10, -10);
                
                playerCountText = countGO.GetComponent<TextMeshProUGUI>();
                playerCountText.fontSize = 16;
                playerCountText.alignment = TextAlignmentOptions.Center;
                playerCountText.text = "Players: 0";
            }
        }
        
        private void InitializeKickUI()
        {
            // Ensure we have an EventSystem in the scene for button clicks
            if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                Debug.Log("[FusionPlayersDisplay] Adding EventSystem for UI interaction");
                GameObject eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            // Create kick confirmation panel if not assigned
            if (kickConfirmationPanel == null)
            {
                kickConfirmationPanel = CreateConfirmationPanel();
                kickConfirmationPanel.SetActive(false);
            }
            
            // Create kick result panel if not assigned
            if (kickResultPanel == null)
            {
                kickResultPanel = CreateResultPanel();
                kickResultPanel.SetActive(false);
            }

            // Add a blocking background to prevent clicks through panels
            GameObject blockingBg = new GameObject("BlockingBackground", typeof(RectTransform), typeof(Image));
            blockingBg.transform.SetParent(transform, false);
            blockingBg.transform.SetAsFirstSibling(); // Put it behind everything
            
            RectTransform blockingRect = blockingBg.GetComponent<RectTransform>();
            blockingRect.anchorMin = Vector2.zero;
            blockingRect.anchorMax = Vector2.one;
            blockingRect.offsetMin = Vector2.zero;
            blockingRect.offsetMax = Vector2.zero;
            
            Image blockingImage = blockingBg.GetComponent<Image>();
            blockingImage.color = new Color(0, 0, 0, 0.5f);
            blockingImage.raycastTarget = true;
            
            // Make this our background blocker
            blockingBg.SetActive(false);
            _blockingBackground = blockingBg;
        }

        private GameObject CreateConfirmationPanel()
        {
            // Create panel with canvas component to ensure proper event handling
            GameObject panelGO = new GameObject("KickConfirmationPanel", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            panelGO.transform.SetParent(transform, false);
            
            RectTransform panelRect = panelGO.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.3f, 0.4f);
            panelRect.anchorMax = new Vector2(0.7f, 0.6f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            
            Image panelImage = panelGO.GetComponent<Image>();
            panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
            panelImage.raycastTarget = true;
            
            // Add a canvas group for better interaction
            CanvasGroup canvasGroup = panelGO.GetComponent<CanvasGroup>();
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.alpha = 1f;
            
            // Add confirmation text
            GameObject textGO = new GameObject("ConfirmationText", typeof(RectTransform), typeof(TextMeshProUGUI));
            textGO.transform.SetParent(panelGO.transform, false);
            
            RectTransform textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0, 0.6f);
            textRect.anchorMax = new Vector2(1, 0.9f);
            textRect.offsetMin = new Vector2(10, 0);
            textRect.offsetMax = new Vector2(-10, 0);
            
            kickConfirmationText = textGO.GetComponent<TextMeshProUGUI>();
            kickConfirmationText.fontSize = 16;
            kickConfirmationText.alignment = TextAlignmentOptions.Center;
            kickConfirmationText.text = "Are you sure you want to kick this player?";
            kickConfirmationText.raycastTarget = false; // Don't block clicks
            
            // Use a simplified approach for buttons - direct children without layout group
            // Create confirm button
            GameObject confirmGO = CreateButton("ConfirmButton", panelGO.transform, 
                new Vector2(0.25f, 0.25f), new Vector2(0.45f, 0.5f),
                new Color(0.8f, 0.2f, 0.2f, 1f), "Kick");
            confirmKickButton = confirmGO.GetComponent<Button>();
            
            // Create cancel button
            GameObject cancelGO = CreateButton("CancelButton", panelGO.transform, 
                new Vector2(0.55f, 0.25f), new Vector2(0.75f, 0.5f),
                new Color(0.3f, 0.3f, 0.3f, 1f), "Cancel");
            cancelKickButton = cancelGO.GetComponent<Button>();
            
            // Set up button actions explicitly  
            confirmKickButton.onClick.AddListener(ConfirmKickPlayer);
            cancelKickButton.onClick.AddListener(CancelKickPlayer);
            
            return panelGO;
        }
        
        // Helper method to create buttons reliably
        private GameObject CreateButton(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Color color, string text)
        {
            GameObject buttonGO = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonGO.transform.SetParent(parent, false);
            
            RectTransform buttonRect = buttonGO.GetComponent<RectTransform>();
            buttonRect.anchorMin = anchorMin;
            buttonRect.anchorMax = anchorMax;
            buttonRect.offsetMin = Vector2.zero;
            buttonRect.offsetMax = Vector2.zero;
            
            Image buttonImage = buttonGO.GetComponent<Image>();
            buttonImage.color = color;
            buttonImage.raycastTarget = true;
            
            Button button = buttonGO.GetComponent<Button>();
            
            // Set up button colors for better visual feedback
            ColorBlock colors = button.colors;
            colors.normalColor = color;
            colors.highlightedColor = new Color(
                Mathf.Min(color.r + 0.2f, 1f),
                Mathf.Min(color.g + 0.2f, 1f),
                Mathf.Min(color.b + 0.2f, 1f),
                1f);
            colors.pressedColor = new Color(
                Mathf.Max(color.r - 0.2f, 0f),
                Mathf.Max(color.g - 0.2f, 0f),
                Mathf.Max(color.b - 0.2f, 0f),
                1f);
            button.colors = colors;
            
            // Add button text
            GameObject textGO = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            textGO.transform.SetParent(buttonGO.transform, false);
            
            RectTransform textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            TextMeshProUGUI buttonText = textGO.GetComponent<TextMeshProUGUI>();
            buttonText.text = text;
            buttonText.fontSize = 14;
            buttonText.color = Color.white;
            buttonText.alignment = TextAlignmentOptions.Center;
            buttonText.raycastTarget = false; // Don't block clicks on the button itself
            
            return buttonGO;
        }
        
        private GameObject CreateResultPanel()
        {
            // Create panel
            GameObject panelGO = new GameObject("KickResultPanel", typeof(RectTransform), typeof(Image));
            panelGO.transform.SetParent(transform, false);
            
            RectTransform panelRect = panelGO.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.3f, 0.45f);
            panelRect.anchorMax = new Vector2(0.7f, 0.55f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            
            Image panelImage = panelGO.GetComponent<Image>();
            panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
            
            // Add result text
            GameObject textGO = new GameObject("ResultText", typeof(RectTransform), typeof(TextMeshProUGUI));
            textGO.transform.SetParent(panelGO.transform, false);
            
            RectTransform textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10, 0);
            textRect.offsetMax = new Vector2(-10, 0);
            
            kickResultText = textGO.GetComponent<TextMeshProUGUI>();
            kickResultText.fontSize = 16;
            kickResultText.alignment = TextAlignmentOptions.Center;
            
            return panelGO;
        }

        public override void Spawned()
        {
            base.Spawned();
            runner = Object.Runner;
            
            // Subscribe to player events
            NetworkPlayer.EventPlayerSpawned += OnPlayerSpawned;
            NetworkPlayer.EventPlayerDespawned += OnPlayerDespawned;
            
            Debug.Log("[FusionPlayersDisplay] Initialized in Shared Mode");
            
            // Initial refresh to show existing players
            RefreshPlayersList();
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            base.Despawned(runner, hasState);
            
            // Unsubscribe from events
            NetworkPlayer.EventPlayerSpawned -= OnPlayerSpawned;
            NetworkPlayer.EventPlayerDespawned -= OnPlayerDespawned;
            
            // Clean up UI
            ClearAllPlayerEntries();
        }

        private void Update()
        {
            if (runner == null || !runner.IsRunning) return;
            
            refreshTimer += Time.deltaTime;
            if (refreshTimer >= refreshInterval && !isKickPanelOpen)
            {
                RefreshPlayersList();
                refreshTimer = 0f;
            }
        }

        private void OnPlayerSpawned(NetworkPlayer player)
        {
            RefreshPlayersList();
        }

        private void OnPlayerDespawned(NetworkPlayer player)
        {
            RefreshPlayersList();
        }

        private void RefreshPlayersList()
        {
            if (runner == null || !runner.IsRunning) return;

            // Get all active players
            var activePlayers = runner.ActivePlayers.ToList();
            
            // Update count text
            if (playerCountText != null)
            {
                playerCountText.text = $"Players: {activePlayers.Count}";
            }
            
            if (logPlayersOnRefresh)
            {
                Debug.Log($"[FusionPlayersDisplay] Total players: {activePlayers.Count}");
            }

            // Remove entries for players no longer in the room
            List<PlayerRef> playersToRemove = new List<PlayerRef>();
            foreach (var entry in playerEntries)
            {
                if (!activePlayers.Contains(entry.Key))
                {
                    playersToRemove.Add(entry.Key);
                }
            }

            foreach (var player in playersToRemove)
            {
                RemovePlayerEntry(player);
            }

            // Add entries for new players
            foreach (var player in activePlayers)
            {
                if (!playerEntries.ContainsKey(player))
                {
                    CreatePlayerEntry(player);
                }
                else
                {
                    // Update existing entry
                    UpdatePlayerEntry(player);
                }
            }
        }

        private void CreatePlayerEntry(PlayerRef player)
        {
            if (contentParent == null) return;

            GameObject entryGO;
            
            // Use prefab if provided, otherwise create a dynamic entry
            if (playerEntryPrefab != null)
            {
                entryGO = Instantiate(playerEntryPrefab, contentParent);
            }
            else
            {
                entryGO = PlayerEntryUI.CreatePlayerEntryGameObject(contentParent);
            }

            // Set layout element properties for proper sizing
            if (entryGO.GetComponent<LayoutElement>() == null)
            {
                LayoutElement layoutElement = entryGO.AddComponent<LayoutElement>();
                layoutElement.minHeight = 40;
                layoutElement.flexibleWidth = 1;
            }

            // Update the entry with player info
            UpdateEntryWithPlayerInfo(entryGO, player);

            playerEntries.Add(player, entryGO);
        }

        private void UpdatePlayerEntry(PlayerRef player)
        {
            if (playerEntries.TryGetValue(player, out GameObject entryGO))
            {
                UpdateEntryWithPlayerInfo(entryGO, player);
            }
        }

        private void UpdateEntryWithPlayerInfo(GameObject entryGO, PlayerRef player)
        {
            bool isLocal = player == runner.LocalPlayer;
            
            // Try to get player name from NetworkPlayer
            string playerName = "Unknown";
            string playerUid = string.Empty;
            
            NetworkPlayer networkPlayer = FindNetworkPlayerByRef(player);
            if (networkPlayer != null)
            {
                playerName = networkPlayer.Username.Value;
                
                // Try to get UID if available
                var playerUIDSharer = networkPlayer.GetComponent<PlayerUIDSharer>();
                if (playerUIDSharer != null && !string.IsNullOrEmpty(playerUIDSharer.NetworkedUID.Value))
                {
                    playerUid = playerUIDSharer.NetworkedUID.Value;
                }
            }

            // Update UI using PlayerEntryUI if available
            PlayerEntryUI entryUI = entryGO.GetComponent<PlayerEntryUI>();
            if (entryUI != null)
            {
                entryUI.SetPlayerInfo(playerName, player.ToString(), isLocal, player);
                
                // Set up kick button callback
                if (!isLocal)
                {
                    entryUI.SetKickButtonCallback(() => ShowKickConfirmation(player, playerName));
                }
            }
            else
            {
                // Fallback to updating TextMeshProUGUI directly
                TextMeshProUGUI text = entryGO.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                {
                    string displayText = isLocal ? "➤ " : "";
                    displayText += $"{playerName} (ID: {player})";
                    
                    if (showDetailedPlayerInfo && !string.IsNullOrEmpty(playerUid))
                    {
                        displayText += $" UID: {playerUid}";
                    }
                    
                    text.text = displayText;
                }
            }
        }

        private void ShowKickConfirmation(PlayerRef player, string playerName)
        {
            if (kickConfirmationPanel == null) return;
            
            playerToKick = player;
            kickConfirmationText.text = $"Are you sure you want to kick {playerName}?";
            
            // Show blocking background
            if (_blockingBackground != null)
            {
                _blockingBackground.SetActive(true);
            }
            
            kickConfirmationPanel.SetActive(true);
            isKickPanelOpen = true;
            
            // Force select the cancel button by default (safer option)
            if (cancelKickButton != null)
            {
                cancelKickButton.Select();
            }
        }
        
        private void ConfirmKickPlayer()
        {
            if (runner == null || !runner.IsRunning) return;
            
            // Check if we have authority to kick
            if (HasStateAuthority)
            {
                KickPlayer_RPC(playerToKick);
            }
            else
            {
                // Request the server to kick the player
                RPC_RequestKickPlayer(playerToKick);
            }
            
            // Hide confirmation panel
            kickConfirmationPanel.SetActive(false);
            if (_blockingBackground != null)
            {
                _blockingBackground.SetActive(false);
            }
            isKickPanelOpen = false;
        }
        
        private void CancelKickPlayer()
        {
            kickConfirmationPanel.SetActive(false);
            if (_blockingBackground != null)
            {
                _blockingBackground.SetActive(false);
            }
            isKickPanelOpen = false;
            playerToKick = default;
        }
        
        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_RequestKickPlayer(PlayerRef playerToKick)
        {
            // Only the state authority can actually kick players
            if (HasStateAuthority)
            {
                KickPlayer_RPC(playerToKick);
            }
        }
        
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void KickPlayer_RPC(PlayerRef playerToKick)
        {
            NetworkPlayer kickedPlayer = FindNetworkPlayerByRef(playerToKick);
            string playerName = (kickedPlayer != null) ? kickedPlayer.Username.Value : playerToKick.ToString();
            
            // If this is the player being kicked
            if (playerToKick == runner.LocalPlayer)
            {
                StartCoroutine(HandleKickedLocally(playerName));
            }
            else
            {
                // Show kick result for other players
                ShowKickResult($"{playerName} was kicked from the room");
            }
        }
        
        private IEnumerator HandleKickedLocally(string playerName)
        {
            // Show kicked message
            ShowKickResult("You have been kicked from the room");
            
            // Wait a moment for the message to be read
            yield return new WaitForSeconds(2f);
            
            // Disconnect and return to the main menu or reconnect scene
            try
            {
                // Shutdown the network runner
                if (runner != null)
                {
                    runner.Shutdown();
                }
                
                // Try to clean up any persistent Fusion-related objects
                // Look for any NetworkRunner instances
                var networkRunners = FindObjectsOfType<NetworkRunner>();
                foreach (var nr in networkRunners)
                {
                    if (nr != runner) // Avoid double-destroying our own runner
                    {
                        Destroy(nr.gameObject);
                    }
                }
                
                // Look for any NetworkManager instances
                var networkManagers = FindObjectsOfType<NetworkManager>();
                if (networkManagers.Length > 0)
                {
                    foreach (var manager in networkManagers)
                    {
                        Destroy(manager.gameObject);
                    }
                }
                
                // Load the main menu or connection scene
                // Use the first scene in the build index, which is typically the main menu
                SceneManager.LoadScene(0);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[FusionPlayersDisplay] Error handling player kick: {e.Message}");
            }
        }
        
        private void ShowKickResult(string message)
        {
            kickResultText.text = message;
            kickResultPanel.SetActive(true);
            
            // Hide after delay
            StartCoroutine(HideKickResultAfterDelay());
        }
        
        private IEnumerator HideKickResultAfterDelay()
        {
            yield return new WaitForSeconds(resultDisplayTime);
            kickResultPanel.SetActive(false);
        }

        private void RemovePlayerEntry(PlayerRef player)
        {
            if (playerEntries.TryGetValue(player, out GameObject entry))
            {
                Destroy(entry);
                playerEntries.Remove(player);
            }
        }

        private void ClearAllPlayerEntries()
        {
            foreach (var entry in playerEntries.Values)
            {
                Destroy(entry);
            }
            
            playerEntries.Clear();
            
            if (playerCountText != null)
            {
                playerCountText.text = "Players: 0";
            }
        }

        private NetworkPlayer FindNetworkPlayerByRef(PlayerRef playerRef)
        {
            NetworkPlayer[] players = FindObjectsOfType<NetworkPlayer>();
            return players.FirstOrDefault(p => p.Object.InputAuthority == playerRef);
        }

        public void FixKickButtons()
        {
            // This method can be called from the Inspector to fix button issues
            Debug.Log("[FusionPlayersDisplay] Recreating kick buttons...");
            
            if (kickConfirmationPanel != null)
            {
                // Destroy the old panels
                Destroy(kickConfirmationPanel);
                if (kickResultPanel != null) Destroy(kickResultPanel);
                if (_blockingBackground != null) Destroy(_blockingBackground);
                
                // Recreate them
                InitializeKickUI();
                
                Debug.Log("[FusionPlayersDisplay] Kick buttons recreated!");
            }
        }
    }
} 
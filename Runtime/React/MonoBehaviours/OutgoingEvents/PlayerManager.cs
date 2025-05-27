using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Spaces.React.Runtime;
using NinjutsuGames.FusionNetwork.Runtime;
using System.Runtime.InteropServices;
using Fusion;

[System.Serializable]
public class PlayerInfo
{
    public string playerName;
    public string playerId;
    public bool isLocalPlayer;
    public string uid;
}

[System.Serializable]
public class PlayerListData
{
    public List<PlayerInfo> players;
}

public class PlayerManager : NetworkBehaviour, IStateAuthorityChanged
{
    [DllImport("__Internal")]
    private static extern void JsUpdatePlayerList(string jsonData);

    [Networked]
    private NetworkDictionary<NetworkString<_32>, NetworkString<_128>> NetworkedPlayerUids { get; }

    private List<NetworkPlayer> currentPlayers = new List<NetworkPlayer>();
    private Dictionary<string, string> playerUids = new Dictionary<string, string>();
    private bool initialized = false;
    private bool dictionaryInitialized = false;

    private void OnEnable()
    {
        NetworkManager.EventGameStarted += OnGameStarted;
        NetworkPlayer.EventPlayerSpawned += OnPlayerSpawned;
        NetworkPlayer.EventPlayerDespawned += OnPlayerDespawned;
        ReactIncomingEvent.OnReceivedFirebaseUser += HandleOnReceivedFirebaseUser;
        PlayerUIDSharer.OnPlayerUIDUpdated += HandlePlayerUIDUpdated;
    }

    private void OnDisable()
    {
        NetworkManager.EventGameStarted -= OnGameStarted;
        NetworkPlayer.EventPlayerSpawned -= OnPlayerSpawned;
        NetworkPlayer.EventPlayerDespawned -= OnPlayerDespawned;
        ReactIncomingEvent.OnReceivedFirebaseUser -= HandleOnReceivedFirebaseUser;
        PlayerUIDSharer.OnPlayerUIDUpdated -= HandlePlayerUIDUpdated;
    }

    private void Start()
    {
        Debug.Log("PlayerManager: Started");
        StartCoroutine(InitializePlayerList());
    }

    private IEnumerator InitializePlayerList()
    {
        Debug.Log("PlayerManager: Starting initialization delay");
        yield return new WaitForSeconds(0.5f);

        // Clear existing lists to ensure fresh start
        currentPlayers.Clear();
        playerUids.Clear();

        var localPlayer = NetworkPlayer.LocalPlayer;
        if (localPlayer != null && !currentPlayers.Contains(localPlayer))
        {
            currentPlayers.Add(localPlayer);
            Debug.Log($"PlayerManager: Added local player during initialization: {localPlayer.Username.Value}");
        }
        else
        {
            Debug.Log("PlayerManager: No local player found during initialization");
        }

        initialized = true;
        UpdatePlayerList();
    }

    private void OnGameStarted()
    {
        Debug.Log("PlayerManager: Game started");
        UpdatePlayerList();
    }

    private void OnPlayerSpawned(NetworkPlayer player)
    {
        Debug.Log($"PlayerManager: Player Spawned - {player.Username.Value}");
        if (!currentPlayers.Contains(player))
        {
            currentPlayers.Add(player);
            UpdatePlayerList();
        }
    }

    private void OnPlayerDespawned(NetworkPlayer player)
    {
        Debug.Log($"PlayerManager: Player Despawned - {player.Username.Value}");
        if (currentPlayers.Contains(player))
        {
            currentPlayers.Remove(player);
            string playerId = player.Object.InputAuthority.ToString();
            
            // Also remove from the networked dictionary if we have state authority
            if (Object.HasStateAuthority && playerUids.ContainsKey(playerId))
            {
                try {
                    NetworkedPlayerUids.Remove(new NetworkString<_32>(playerId));
                    Debug.Log($"PlayerManager: Removed player from NetworkedPlayerUids: {playerId}");
                }
                catch (System.Exception e) {
                    Debug.LogWarning($"PlayerManager: Failed to remove from NetworkedPlayerUids: {e.Message}");
                }
                
                playerUids.Remove(playerId);
            }
            
            Debug.Log($"PlayerManager: Removed player. Current players: {string.Join(", ", currentPlayers.Select(p => p.Username.Value))}");
            
            // Force re-initialization if all players are gone
            if (currentPlayers.Count == 0)
            {
                initialized = false;
                StartCoroutine(InitializePlayerList());
            }
            else
            {
                UpdatePlayerList();
            }
        }
    }

    private void HandleOnReceivedFirebaseUser(FirebaseUserData userData)
    {
        var localPlayer = NetworkPlayer.LocalPlayer;
        if (localPlayer != null)
        {
            string playerId = localPlayer.Object.InputAuthority.ToString();
            playerUids[playerId] = userData.uid;
            if (Object.HasStateAuthority)
            {
                NetworkedPlayerUids.Set(playerId, userData.uid);
            }
            Debug.Log($"PlayerManager: Received Firebase UID for local player: {userData.uid}");
            UpdatePlayerList();
        }
    }

    // This method is called when PlayerUIDSharer fires an update event.
    private void HandlePlayerUIDUpdated(string playerId, string uid)
    {
        Debug.Log($"PlayerManager: Received UID update for player {playerId}: {uid}");
        SetPlayerUID(playerId, uid);
    }

    private void UpdatePlayerList()
    {
        if (!initialized)
        {
            Debug.Log("PlayerManager: Not initialized yet, skipping update");
            return;
        }

        Debug.Log($"PlayerManager: Updating player list. Total players: {currentPlayers.Count}");
        List<PlayerInfo> playerInfoList = new List<PlayerInfo>();
        HashSet<string> processedPlayerIds = new HashSet<string>();

        // First add local player if available.
        var localPlayer = NetworkPlayer.LocalPlayer;
        if (localPlayer != null && currentPlayers.Contains(localPlayer))
        {
            string playerId = localPlayer.Object.InputAuthority.ToString();
            if (!processedPlayerIds.Contains(playerId))
            {
                string uid = ReactUnityFieldHandler.CurrentUserUID;
                if (!string.IsNullOrEmpty(uid))
                {
                    playerUids[playerId] = uid;
                }

                Debug.Log($"PlayerManager: Adding local player first - Username: {localPlayer.Username.Value}, ID: {playerId}, UID: {uid}");
                playerInfoList.Add(new PlayerInfo
                {
                    playerName = localPlayer.Username.Value,
                    playerId = playerId,
                    isLocalPlayer = true,
                    uid = playerUids.GetValueOrDefault(playerId, "")
                });
                processedPlayerIds.Add(playerId);
            }
        }

        // Then add all other players.
        foreach (var player in currentPlayers)
        {
            if (player != null && player.Object != null && player != localPlayer)
            {
                string playerId = player.Object.InputAuthority.ToString();
                if (!processedPlayerIds.Contains(playerId))
                {
                    string uid = playerUids.GetValueOrDefault(playerId, "");
                    Debug.Log($"PlayerManager: Adding player to list - Username: {player.Username.Value}, ID: {playerId}, UID: {uid}");
                    playerInfoList.Add(new PlayerInfo
                    {
                        playerName = player.Username.Value,
                        playerId = playerId,
                        isLocalPlayer = player.Object.HasInputAuthority,
                        uid = uid
                    });
                    processedPlayerIds.Add(playerId);
                }
            }
        }

        PlayerListData data = new PlayerListData { players = playerInfoList };
        string jsonData = JsonUtility.ToJson(data);
        Debug.Log("Unity: Sending updated player list: " + jsonData);

#if !UNITY_EDITOR && UNITY_WEBGL
        JsUpdatePlayerList(jsonData);
#endif

        ReactRaiseEvent.UpdatePlayerList(jsonData);
    }

    public override void Spawned()
    {
        base.Spawned();
        
        // Initialize the NetworkedPlayerUids dictionary if we have state authority
        if (Object.HasStateAuthority && !dictionaryInitialized)
        {
            // Clear the dictionary and ensure we don't exceed capacity
            NetworkedPlayerUids.Clear();
            dictionaryInitialized = true;
        }
        
        if (!initialized)
        {
            initialized = true;
            UpdatePlayerList();
        }
    }

    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork();
        
        // If we're not the state authority, sync UIDs from the networked dictionary
        if (!Object.HasStateAuthority && dictionaryInitialized)
        {
            foreach (var kvp in NetworkedPlayerUids)
            {
                string key = kvp.Key.ToString();
                string value = kvp.Value.ToString();
                
                if (!playerUids.ContainsKey(key) || playerUids[key] != value)
                {
                    playerUids[key] = value;
                    Debug.Log($"PlayerManager: Synced UID for player {key}: {value}");
                }
            }
        }
    }

    public void SetPlayerUID(string playerId, string uid)
    {
        Debug.Log($"PlayerManager: Setting UID for player {playerId}: {uid}");
        if (!string.IsNullOrEmpty(uid))
        {
            playerUids[playerId] = uid;
            
            // Only the state authority can modify the networked dictionary
            if (Object.HasStateAuthority)
            {
                // Ensure we have capacity before adding
                EnsureDictionaryCapacity();
                
                NetworkedPlayerUids.Set(new NetworkString<_32>(playerId), new NetworkString<_128>(uid));
                Debug.Log($"PlayerManager: Updated NetworkedPlayerUids for player {playerId}");
            }
            
            UpdatePlayerList();
        }
    }

    public void StateAuthorityChanged()
    {
        if (Object.HasStateAuthority)
        {
            Debug.Log("PlayerManager: Became state authority - syncing player UIDs");
            
            // Sync our local UIDs to the networked dictionary
            foreach (var kvp in playerUids)
            {
                NetworkedPlayerUids.Set(new NetworkString<_32>(kvp.Key), new NetworkString<_128>(kvp.Value));
            }
            
            dictionaryInitialized = true;
        }
    }

    private void EnsureDictionaryCapacity()
    {
        // If we're approaching capacity, remove oldest entries
        if (Object.HasStateAuthority && NetworkedPlayerUids.Count > 25) // Leave some buffer
        {
            Debug.Log("PlayerManager: Dictionary approaching capacity, cleaning up old entries");
            
            // Get keys that aren't in the current players list
            List<NetworkString<_32>> keysToRemove = new List<NetworkString<_32>>();
            
            foreach (var kvp in NetworkedPlayerUids)
            {
                string playerId = kvp.Key.ToString();
                bool playerExists = currentPlayers.Any(p => p.Object.InputAuthority.ToString() == playerId);
                
                if (!playerExists)
                {
                    keysToRemove.Add(kvp.Key);
                }
            }
            
            // Remove old entries
            foreach (var key in keysToRemove)
            {
                NetworkedPlayerUids.Remove(key);
                Debug.Log($"PlayerManager: Removed stale entry from dictionary: {key}");
            }
        }
    }
}

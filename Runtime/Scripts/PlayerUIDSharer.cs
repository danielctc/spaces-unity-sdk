using System.Collections;
using Fusion;
using UnityEngine;
using Spaces.React.Runtime;
using NinjutsuGames.FusionNetwork.Runtime;

public class PlayerUIDSharer : NetworkBehaviour
{
    // Networked variable that holds the player's UID.
    [Networked]
    public NetworkString<_128> NetworkedUID { get; private set; }
    
    private NetworkCharacter networkCharacter;
    private bool isInitialized = false;

    // Static event that fires whenever a player's UID is updated.
    public static event System.Action<string, string> OnPlayerUIDUpdated;

    private string lastNotifiedUID;

    public override void Spawned()
    {
        Debug.Log($"[PlayerUIDSharer] Network object spawned. HasInputAuthority: {Object.HasInputAuthority}, HasStateAuthority: {Object.HasStateAuthority}");
        
        // Start the coroutine to wait for NetworkCharacter
        StartCoroutine(WaitForNetworkCharacter());
        
        // If we're the local player, set our UID
        if (Object.HasInputAuthority)
        {
            StartCoroutine(SetLocalPlayerUID());
        }
        else
        {
            // For remote players, check if we already have a UID
            if (!string.IsNullOrEmpty(NetworkedUID.Value))
            {
                NotifyManager();
            }
        }
    }

    private IEnumerator SetLocalPlayerUID()
    {
        // Wait for UID to be available
        int attempts = 0;
        while (string.IsNullOrEmpty(PopulateUIOnAuth.CurrentUserUID) && attempts < 20)
        {
            yield return new WaitForSeconds(0.5f);
            attempts++;
        }
        
        string uid = PopulateUIOnAuth.CurrentUserUID;
        if (!string.IsNullOrEmpty(uid))
        {
            Debug.Log($"[PlayerUIDSharer] Setting local player UID: {uid}");
            
            // Send RPC to state authority to update the networked UID
            RPC_SendUID(uid);
            
            // Also update locally
            NetworkedUID = uid;
            NotifyManager();
        }
        else
        {
            Debug.LogWarning("[PlayerUIDSharer] Failed to get current user UID after multiple attempts");
        }
    }

    private IEnumerator WaitForNetworkCharacter()
    {
        // Wait for NetworkCharacter.LocalPlayer to be set
        yield return new WaitUntil(() => NetworkCharacter.LocalPlayer != null);
        
        // Get the NetworkCharacter component from this GameObject
        networkCharacter = GetComponent<NetworkCharacter>();
        
        if (networkCharacter == null)
        {
            Debug.Log("[PlayerUIDSharer] Waiting for NetworkCharacter to be available...");
            int attempts = 0;
            while (networkCharacter == null && attempts < 20)
            {
                networkCharacter = GetComponent<NetworkCharacter>();
                if (networkCharacter != null) break;
                attempts++;
                yield return new WaitForSeconds(0.1f);
            }
        }

        if (networkCharacter != null)
        {
            Debug.Log($"[PlayerUIDSharer] Found NetworkCharacter. IsLocal: {Object.HasInputAuthority}");
            InitializePlayer();
        }
        else
        {
            Debug.LogError($"[PlayerUIDSharer] Could not find NetworkCharacter on GameObject: {gameObject.name}");
        }
    }

    private void InitializePlayer()
    {
        if (isInitialized) return;
        isInitialized = true;

        if (Object.HasInputAuthority)
        {
            string uid = PopulateUIOnAuth.CurrentUserUID;
            if (!string.IsNullOrEmpty(uid))
            {
                Debug.Log($"[PlayerUIDSharer] Setting local player UID: {uid}");
                NetworkedUID = uid;
            }
        }
        else
        {
            Debug.Log($"[PlayerUIDSharer] Remote player initialized with UID: {NetworkedUID.Value}");
        }
    }

    // RPC called from the local player to update the networked UID on the state authority.
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_SendUID(string uid, RpcInfo info = default)
    {
        NetworkedUID = uid;
        Debug.Log($"[PlayerUIDSharer] RPC set NetworkedUID to: {uid}");
        // After setting, notify everyone.
        NotifyManager();
    }

    // Modify FixedUpdateNetwork to be more robust
    public override void FixedUpdateNetwork()
    {
        if (string.IsNullOrEmpty(NetworkedUID.Value))
        {
            // If we're the local player and our UID is not set, try to set it
            if (Object.HasInputAuthority)
            {
                string uid = PopulateUIOnAuth.CurrentUserUID;
                if (!string.IsNullOrEmpty(uid) && uid != NetworkedUID.Value)
                {
                    RPC_SendUID(uid);
                }
            }
            return;
        }
        
        // Only notify if the UID has changed
        if (NetworkedUID.Value != lastNotifiedUID)
        {
            lastNotifiedUID = NetworkedUID.Value;
            NotifyManager();
        }
    }

    // Improve the NotifyManager method
    private void NotifyManager()
    {
        if (string.IsNullOrEmpty(NetworkedUID.Value)) return;
        
        string playerId = Object.InputAuthority.ToString();
        Debug.Log($"[PlayerUIDSharer] Notifying manager of UID for player {playerId}: {NetworkedUID.Value}");
        
        // Fire the event so that any subscriber (like PlayerManager) can update
        OnPlayerUIDUpdated?.Invoke(playerId, NetworkedUID.Value);
        
        // Also directly update the PlayerManager if we can find it
        var playerManager = FindObjectOfType<PlayerManager>();
        if (playerManager != null)
        {
            playerManager.SetPlayerUID(playerId, NetworkedUID.Value);
        }
    }
}

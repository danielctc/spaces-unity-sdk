using UnityEngine;
using Spaces.React.Runtime;
using Fusion;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;

// Use NetworkBehaviour for RPCs
public class KickPlayer : NetworkBehaviour
{
    // Keep track if we've subscribed to the event
    private static bool _subscribed = false;

    // Subscribe to the React event when the component is spawned
    public override void Spawned()
    {
        base.Spawned();
        // Ensure subscription happens only once
        if (!_subscribed)
        {
            ReactIncomingEvent.OnReactKickPlayer += HandleKickPlayerRequest;
            _subscribed = true;
            Debug.Log("[KickPlayer] Subscribed to React kick events.");
        }
    }

    // Unsubscribe when despawned or destroyed
    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        base.Despawned(runner, hasState);
        // Consider if unsubscribing is truly needed or handled elsewhere
        // If this object persists, maybe don't unsubscribe here
        // For now, let's assume it might be destroyed
        if (_subscribed && runner != null && runner.IsShutdown) // Check if runner is shutting down
        {
             ReactIncomingEvent.OnReactKickPlayer -= HandleKickPlayerRequest;
             _subscribed = false;
             Debug.Log("[KickPlayer] Unsubscribed from React kick events due to shutdown.");
        }
    }

    private void OnDestroy()
    {
        // Ensure cleanup if the object is destroyed unexpectedly
        if (_subscribed)
        {
             ReactIncomingEvent.OnReactKickPlayer -= HandleKickPlayerRequest;
             _subscribed = false;
             Debug.Log("[KickPlayer] Unsubscribed from React kick events on Destroy.");
        }
    }

    // Static handler to receive the event from React
    private static void HandleKickPlayerRequest(KickPlayerData data)
    {
        Debug.Log($"[KickPlayer] Received kick request for player with UID: {data.uid}");
        
        NetworkRunner runner = FindAnyObjectByType<NetworkRunner>();
        if (runner == null)
        {
            Debug.LogError("[KickPlayer] No NetworkRunner found in the scene.");
            ReactRaiseEvent.SendKickPlayerResult(false, "Unknown", data.uid, "NetworkRunner not found");
            return;
        }

        // Find the target PlayerRef by UID
        PlayerRef targetPlayerRef = PlayerRef.None;
        NetworkObject targetPlayerObject = null;
        string playerName = "Unknown";

        foreach (var playerRef in runner.ActivePlayers)
        {
            if (playerRef == runner.LocalPlayer) continue;

            var uidSharers = FindObjectsOfType<PlayerUIDSharer>();
            foreach (var sharer in uidSharers)
            {
                if (sharer && sharer.Object && sharer.Object.InputAuthority == playerRef)
                {
                    try 
                    { 
                        string uid = sharer.NetworkedUID.Value;
                        if (uid == data.uid)
                        {
                            Debug.Log($"[KickPlayer] Found player {playerRef} with matching UID {data.uid}");
                            targetPlayerRef = playerRef;
                            // IMPORTANT: Get the NetworkObject associated with the player
                            if (runner.TryGetPlayerObject(playerRef, out var playerObj))
                            {
                                targetPlayerObject = playerObj;
                                playerName = "Player " + playerRef.PlayerId;
                            }
                            else 
                            { 
                                Debug.LogWarning($"[KickPlayer] Found PlayerRef {playerRef} but could not get NetworkObject."); 
                            }
                            goto FoundPlayer; 
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError($"[KickPlayer] Error checking UID for player {playerRef}: {ex.Message}");
                    }
                }
            }
        }

    FoundPlayer:
        if (targetPlayerRef != PlayerRef.None && targetPlayerObject != null)
        {
            // Found the player and their NetworkObject
            // Use GetComponentInChildren to find the handler on the prefab or its children
            PlayerKickHandler kickHandler = targetPlayerObject.GetComponentInChildren<PlayerKickHandler>();
            
            if (kickHandler != null)
            {
                Debug.Log($"[KickPlayer] Found PlayerKickHandler on {targetPlayerObject.Id} or its children. Calling Rpc_KickSelf.");
                // Call the RPC on the component attached to the target player's object
                // This RPC will execute on the client owning that object (StateAuthority)
                kickHandler.Rpc_KickSelf(); 
                
                // Send success back to React immediately - we've *initiated* the kick
                ReactRaiseEvent.SendKickPlayerResult(true, playerName, data.uid);
            }
            else
            {
                Debug.LogError($"[KickPlayer] Found player object {targetPlayerObject.Id} but it missing PlayerKickHandler component.");
                ReactRaiseEvent.SendKickPlayerResult(false, playerName, data.uid, "Player object missing kick handler component");
            }
        }
        else
        {
            if (targetPlayerRef != PlayerRef.None && targetPlayerObject == null)
            { 
                Debug.LogError($"[KickPlayer] Found PlayerRef {targetPlayerRef} but couldn't find their NetworkObject.");
                ReactRaiseEvent.SendKickPlayerResult(false, "Unknown", data.uid, "Could not find player's network object");
            }
            else
            { 
                Debug.LogError($"[KickPlayer] No active player found with UID: {data.uid}");
                ReactRaiseEvent.SendKickPlayerResult(false, "Unknown", data.uid, "Player with specified UID not found");
            }
        }
    }
}

// Add this component to player prefabs to help with kicking
public class ShutdownEventDispatcher : MonoBehaviour
{
    public void TriggerShutdownOnLocalClient()
    {
        Debug.Log("[ShutdownEventDispatcher] Shutdown triggered, handling locally");
        StartCoroutine(HandleKickedLocally());
    }
    
    private IEnumerator HandleKickedLocally()
    {
        Debug.Log("[ShutdownEventDispatcher] Preparing to disconnect");
        yield return new WaitForSeconds(1f);
        
        NetworkRunner runner = FindObjectOfType<NetworkRunner>();
        if (runner != null)
        {
            Debug.Log("[ShutdownEventDispatcher] Shutting down NetworkRunner");
            runner.Shutdown();
        }
        
        yield return new WaitForSeconds(0.5f);
        
        Debug.Log("[ShutdownEventDispatcher] Loading scene 0");
        SceneManager.LoadScene(0);
    }
} 
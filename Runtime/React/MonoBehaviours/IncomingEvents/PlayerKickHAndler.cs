using UnityEngine;
using Fusion;
using System.Collections;
using UnityEngine.SceneManagement; // Required for SceneManager

// Attach this component to your Player Prefab
public class PlayerKickHandler : NetworkBehaviour
{
    // RPC called by the KickPlayer manager, executed on this player's client
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void Rpc_KickSelf(RpcInfo info = default)
    {
        Debug.Log($"[PlayerKickHandler] Received Rpc_KickSelf on {Object.Id}. Initiating shutdown.");
        // Important: Check if we actually have StateAuthority before shutting down.
        // This RPC should only execute on the client that owns this object.
        if (Object.HasStateAuthority)
        {
            StartCoroutine(DelayedShutdown());
        }
        else
        {
             Debug.LogWarning($"[PlayerKickHandler] Rpc_KickSelf called on object {Object.Id} but client does not have state authority. Ignoring.");
        }
    }

    private IEnumerator DelayedShutdown()
    {
        // Optional: Add a small delay or show a message before disconnecting
        Debug.Log("[PlayerKickHandler] Shutting down runner in 1 second...");
        yield return new WaitForSeconds(1.0f);

        if (Runner != null && Runner.IsRunning)
        {
            Debug.Log("[PlayerKickHandler] Calling Runner.Shutdown().");
            Runner.Shutdown(); // Disconnect this client

            // Optional: Load a main menu or disconnected scene after shutdown
            // Ensure scene '0' or your desired scene is in build settings
            // You might want to manage scene loading more robustly elsewhere
            // SceneManager.LoadScene(0);
        }
        else
        {
             Debug.LogWarning("[PlayerKickHandler] Runner is null or not running. Cannot shut down.");
        }
    }
}
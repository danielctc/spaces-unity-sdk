#if FUSION_WEAVER
using Fusion;
using UnityEngine;
using NinjutsuGames.FusionNetwork.Runtime;

/// <summary>
/// Attaching this component to a runtime-instantiated object guarantees that there is a valid
/// NetworkObject id which can be used by Game-Creator RPC Instructions.
/// It does so by spawning an (empty/very small) prefab that already contains a NetworkObject
/// registered in the Network Project Config and parenting the runtime object to that instance.
/// 
/// 1. Create a prefab that only contains <see cref="NetworkObject"/> (and, optionally, this script)
///    and add it to Fusion ➜ Network Project Config so it gets a PrefabId.
/// 2. Assign that prefab to <see cref="anchorPrefab"/> in the inspector.
/// 3. Call <see cref="AttachAndSpawn"/> once your visual object is ready (e.g. right after GLB load).
/// </summary>
public class RuntimeNetworkAnchor : MonoBehaviour
{
    [Tooltip("A prefab that contains a single NetworkObject component and is listed in Network Project Config")] 
    [SerializeField] private NetworkObject anchorPrefab;

    /// <summary>
    /// Global fallback prefab. Call <see cref="SetDefaultAnchorPrefab"/> once at boot or before any runtime
    /// anchors are created (e.g. from a manager or downloader) to make sure every anchor can resolve a prefab.
    /// </summary>
    public static NetworkObject DefaultAnchorPrefab { get; private set; }

    public static void SetDefaultAnchorPrefab(NetworkObject prefab)
    {
        DefaultAnchorPrefab = prefab;
    }

    private NetworkObject spawnedAnchor;

    private bool spawnQueued;

    private void OnEnable()
    {
        // Automatically attempt the spawn as early as possible
        QueueSpawn();
    }

    private void QueueSpawn()
    {
        if(spawnQueued) return;
        if (!NetworkManager.IsConnected)
        {
            // Wait until the game session actually starts to avoid null-ref during early simulation setup
            NetworkManager.EventGameStarted += OnGameStarted;
            spawnQueued = true;
            return;
        }
        AttachAndSpawn();
    }

    private void OnGameStarted()
    {
        NetworkManager.EventGameStarted -= OnGameStarted;
        AttachAndSpawn();
    }

    /// <summary>
    /// Spawns the anchor (once) and reparents <c>gameObject</c> under it so the visual picks up the id.
    /// Safe to call multiple times; the spawn will only happen once.
    /// </summary>
    public void AttachAndSpawn()
    {
        if (spawnedAnchor != null) {
            Debug.Log("[RuntimeNetworkAnchor] Already spawned anchor " + spawnedAnchor.Id, this);
            return; // already done
        }
        if (anchorPrefab == null) anchorPrefab = DefaultAnchorPrefab;
        if (anchorPrefab == null)
        {
            // Attempt to load a prefab named "NetworkAnchor" from Resources as a last resort
            anchorPrefab = Resources.Load<NetworkObject>("NetworkAnchor");
        }

        if (anchorPrefab == null)
        {
            Debug.LogError("[RuntimeNetworkAnchor] No anchor prefab assigned – cannot create NetworkObject id (tried DefaultAnchorPrefab and Resources/NetworkAnchor).", this);
            return;
        }

        var runner = FindAnyObjectByType<NetworkRunner>();
        if (runner == null || !runner.IsRunning)
        {
            Debug.LogWarning("[RuntimeNetworkAnchor] No active NetworkRunner yet – queueing.", this);
            StartCoroutine(WaitForRunner());
            return;
        }

        Debug.Log($"[RuntimeNetworkAnchor] Spawning anchor prefab '{anchorPrefab.name}' via runner.Spawn", this);

        spawnedAnchor = runner.Spawn(anchorPrefab, transform.position, transform.rotation, runner.LocalPlayer,
            (r, obj) =>
            {
                // Parent the visual under the freshly spawned anchor so they move together
                transform.SetParent(obj.transform, true);
            });

        if (spawnedAnchor == null)
        {
            Debug.LogError("[RuntimeNetworkAnchor] Runner.Spawn returned null – check Network Project Config.", this);
        }
        else {
            Debug.Log("[RuntimeNetworkAnchor] Anchor spawned with id " + spawnedAnchor.Id, spawnedAnchor);
        }
    }

    private System.Collections.IEnumerator WaitForRunner()
    {
        NetworkRunner runner;
        do
        {
            runner = FindAnyObjectByType<NetworkRunner>();
            yield return null;
        } while (runner == null || !runner.IsRunning);

        AttachAndSpawn();
    }
}
#endif 
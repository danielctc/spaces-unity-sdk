using UnityEngine;
using System.Linq;
#if FUSION_WEAVER
using Fusion;
#endif
using System.Collections;

public class HotspotDetector : MonoBehaviour
{
    [SerializeField] private GameObject testPrefab; // Assign this in the inspector
    [SerializeField] private bool networkSpawn = true; // Kept always for consistent serialization
#if FUSION_WEAVER
    private NetworkRunner runner;
#endif
    private void Awake()
    {
#if FUSION_WEAVER
        if (networkSpawn)
        {
            runner = FindAnyObjectByType<NetworkRunner>();
            if (runner == null)
            {
                Debug.LogWarning("[HotspotDetector] NetworkRunner not found. Falling back to local instantiation.");
            }
        }
#endif
    }
    private void Start()
    {
#if FUSION_WEAVER
        if (networkSpawn)
        {
            // If we need a NetworkRunner but it is not yet available or running, wait for it
            if (runner == null || !runner.IsRunning)
            {
                StartCoroutine(WaitForRunnerThenDetect());
                return;
            }
        }
#endif
        DetectHotspots();
    }

#if FUSION_WEAVER
    private IEnumerator WaitForRunnerThenDetect()
    {
        while (runner == null || !runner.IsRunning)
        {
            runner = FindAnyObjectByType<NetworkRunner>();
            yield return null;
        }

        DetectHotspots();
    }
#endif

    public void DetectHotspots()
    {
        if (testPrefab == null)
        {
            Debug.LogError("[HotspotDetector] Test prefab not assigned!");
            return;
        }

        if (runner == null) {
            StartCoroutine(WaitForRunnerThenDetect());
            return;
        }

        var hotspots = FindObjectsOfType<Transform>()
            .Where(t => t.name.EndsWith("-hotspot"))
            .ToList();

        foreach (var hotspot in hotspots)
        {
            // Prevent duplicate
            if (hotspot.childCount > 0 && hotspot.GetComponentsInChildren<Transform>()
                    .Any(c => c.gameObject.name.StartsWith(testPrefab.name)))
            {
                continue;
            }
#if FUSION_WEAVER
            if (networkSpawn && runner != null && runner.IsRunning && testPrefab.GetComponent<NetworkObject>() != null)
            {
                // runner running ensured
                var netObj = runner.Spawn(testPrefab, hotspot.position, hotspot.rotation, runner.LocalPlayer, (runnerInstance, networkObj) =>
                {
                    Debug.Log($"[HotspotDetector] Spawning network object for hotspot {hotspot.name}");
                });

                if (netObj != null)
                {
                    Debug.Log($"[HotspotDetector] Spawn succeeded id: {netObj.Id}");
                }

                // If the spawn failed for any reason, retry once the runner is ready instead of falling back to a local instantiate
                if (netObj == null)
                {
                    Debug.LogWarning($"[HotspotDetector] Network spawn failed for {hotspot.name}. Will retry once runner is ready.");
                    StartCoroutine(SpawnWhenRunnerReady(hotspot));
                }
            }
            else
#endif
            {
                var local = Instantiate(testPrefab, hotspot.position, hotspot.rotation, hotspot);
#if FUSION_WEAVER && !UNITY_EDITOR
                var nObj = local.GetComponent<NetworkObject>();
                if (nObj != null) {
                    // Give the locally-instantiated object a real NetworkId
                    if (runner != null && runner.IsRunning) {
                        runner.Spawn(nObj);
                        Debug.Log($"[HotspotDetector] Spawn( existing ) id: {nObj.Id}");
                    } else {
                        // runner not ready yet → queue it
                        StartCoroutine(RegisterWhenRunnerReady(nObj));
                    }
                }
#endif
                Debug.Log($"[HotspotDetector] Local instantiate of {local.name} id: {(local.GetComponent<NetworkObject>()?local.GetComponent<NetworkObject>().Id.ToString():"NoNetObj")}");
            }
        }
    }

#if FUSION_WEAVER
    private System.Collections.IEnumerator SpawnWhenRunnerReady(Transform hotspot)
    {
        // Wait until the runner exists and is running
        while (runner == null || !runner.IsRunning)
        {
            yield return null;
        }

        if (hotspot == null) yield break; // Hotspot might have been destroyed meanwhile

        // Prevent duplicate network objects once the runner is ready
        if (hotspot.childCount > 0 && hotspot.GetComponentsInChildren<Transform>()
                .Any(c => c.gameObject.name.StartsWith(testPrefab.name)))
        {
            yield break;
        }

        var netObj = runner.Spawn(testPrefab, hotspot.position, hotspot.rotation, runner.LocalPlayer, (runnerInstance, networkObj) =>
        {
            Debug.Log($"[HotspotDetector] (Delayed) Spawning network object for hotspot {hotspot.name}");
        });

        if (netObj == null)
        {
            Debug.LogError($"[HotspotDetector] Failed to spawn network object for hotspot {hotspot.name} even after runner started.");
        }
    }
#endif

#if FUSION_WEAVER
    private IEnumerator RegisterWhenRunnerReady(NetworkObject nObj) {
        while (runner == null || !runner.IsRunning)
            yield return null;

        if (nObj && !nObj.Id.IsValid)
            runner.Spawn(nObj);
    }
#endif
} 
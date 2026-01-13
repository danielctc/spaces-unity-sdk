using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spaces.React.Runtime.Bridge
{
    /// <summary>
    /// SpacesBridge - Main entry point for Unity thin client architecture.
    ///
    /// Follows Spatial.io pattern:
    /// - React/Platform handles all networking, state sync, and game logic
    /// - Unity handles rendering, input capture, and local physics
    /// - All state flows from React → Unity (no networking code in Unity)
    /// </summary>
    public class SpacesBridge : MonoBehaviour
    {
        public static SpacesBridge Instance { get; private set; }

        [Header("State")]
        public BridgeState State { get; private set; } = BridgeState.Disconnected;

        [Header("Settings")]
        [Tooltip("Interpolation speed for remote actor positions")]
        public float positionInterpolationSpeed = 10f;

        [Tooltip("Interpolation speed for remote actor rotations")]
        public float rotationInterpolationSpeed = 10f;

        [Header("Prefabs")]
        [Tooltip("Prefab for remote actors/players")]
        public GameObject remoteActorPrefab;

        [Tooltip("Default prefab for spawned objects")]
        public GameObject defaultObjectPrefab;

        // Tracked actors and objects
        private Dictionary<string, RemoteActor> remoteActors = new Dictionary<string, RemoteActor>();
        private Dictionary<string, NetworkedObject> networkedObjects = new Dictionary<string, NetworkedObject>();

        // Local actor info
        private string localActorId;
        private Transform localActorTransform;

        // Events
        public static event Action<BridgeState> OnStateChanged;
        public static event Action<string, RemoteActor> OnActorJoined;
        public static event Action<string> OnActorLeft;
        public static event Action<string, NetworkedObject> OnObjectSpawned;
        public static event Action<string> OnObjectDespawned;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Debug.Log("[SpacesBridge] Instance created");
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnEnable()
        {
            // Subscribe to React events
            ReactIncomingEvent.OnBridgeConnect += HandleConnect;
            ReactIncomingEvent.OnBridgeDisconnect += HandleDisconnect;
            ReactIncomingEvent.OnActorJoined += HandleActorJoined;
            ReactIncomingEvent.OnActorLeft += HandleActorLeft;
            ReactIncomingEvent.OnActorUpdate += HandleActorUpdate;
            ReactIncomingEvent.OnObjectSpawned += HandleObjectSpawned;
            ReactIncomingEvent.OnObjectDespawned += HandleObjectDespawned;
            ReactIncomingEvent.OnObjectUpdate += HandleObjectUpdate;
        }

        private void OnDisable()
        {
            // Unsubscribe from React events
            ReactIncomingEvent.OnBridgeConnect -= HandleConnect;
            ReactIncomingEvent.OnBridgeDisconnect -= HandleDisconnect;
            ReactIncomingEvent.OnActorJoined -= HandleActorJoined;
            ReactIncomingEvent.OnActorLeft -= HandleActorLeft;
            ReactIncomingEvent.OnActorUpdate -= HandleActorUpdate;
            ReactIncomingEvent.OnObjectSpawned -= HandleObjectSpawned;
            ReactIncomingEvent.OnObjectDespawned -= HandleObjectDespawned;
            ReactIncomingEvent.OnObjectUpdate -= HandleObjectUpdate;
        }

        private void Update()
        {
            if (State != BridgeState.Connected) return;

            // Interpolate remote actors
            foreach (var actor in remoteActors.Values)
            {
                if (actor.gameObject != null)
                {
                    actor.Interpolate(positionInterpolationSpeed, rotationInterpolationSpeed, Time.deltaTime);
                }
            }

            // Send local actor position to React (throttled internally)
            if (localActorTransform != null)
            {
                SendLocalActorTransform();
            }
        }

        #region Public API

        /// <summary>
        /// Set the local actor's transform (typically the player)
        /// </summary>
        public void SetLocalActor(string actorId, Transform actorTransform)
        {
            localActorId = actorId;
            localActorTransform = actorTransform;
            Debug.Log($"[SpacesBridge] Local actor set: {actorId}");
        }

        /// <summary>
        /// Get a remote actor by ID
        /// </summary>
        public RemoteActor GetRemoteActor(string actorId)
        {
            return remoteActors.TryGetValue(actorId, out var actor) ? actor : null;
        }

        /// <summary>
        /// Get all remote actors
        /// </summary>
        public IEnumerable<RemoteActor> GetRemoteActors()
        {
            return remoteActors.Values;
        }

        /// <summary>
        /// Get a networked object by ID
        /// </summary>
        public NetworkedObject GetNetworkedObject(string objectId)
        {
            return networkedObjects.TryGetValue(objectId, out var obj) ? obj : null;
        }

        /// <summary>
        /// Request to spawn an object (sends request to React)
        /// </summary>
        public void RequestSpawn(string objectType, Vector3 position, Quaternion rotation, string prefabId = null)
        {
            BridgeRaiseEvent.RequestSpawn(new SpawnRequestData
            {
                objectType = objectType,
                position = new Vector3Data { x = position.x, y = position.y, z = position.z },
                rotation = new Vector3Data { x = rotation.eulerAngles.x, y = rotation.eulerAngles.y, z = rotation.eulerAngles.z },
                prefabId = prefabId
            });
        }

        /// <summary>
        /// Request to despawn an object (sends request to React)
        /// </summary>
        public void RequestDespawn(string objectId)
        {
            BridgeRaiseEvent.RequestDespawn(new DespawnRequestData { objectId = objectId });
        }

        /// <summary>
        /// Send a custom event to React
        /// </summary>
        public void SendEvent(string eventName, string jsonData)
        {
            BridgeRaiseEvent.SendCustomEvent(new CustomEventData
            {
                eventName = eventName,
                data = jsonData
            });
        }

        #endregion

        #region Event Handlers

        private void HandleConnect(BridgeConnectData data)
        {
            localActorId = data.localActorId;
            SetState(BridgeState.Connected);
            Debug.Log($"[SpacesBridge] Connected to {data.spaceId}/{data.instanceId} as {data.localActorId}");
        }

        private void HandleDisconnect(BridgeDisconnectData data)
        {
            // Clean up all actors and objects
            foreach (var actor in remoteActors.Values)
            {
                if (actor.gameObject != null)
                {
                    Destroy(actor.gameObject);
                }
            }
            remoteActors.Clear();

            foreach (var obj in networkedObjects.Values)
            {
                if (obj.gameObject != null)
                {
                    Destroy(obj.gameObject);
                }
            }
            networkedObjects.Clear();

            localActorId = null;
            SetState(BridgeState.Disconnected);
            Debug.Log("[SpacesBridge] Disconnected");
        }

        private void HandleActorJoined(ActorJoinedData data)
        {
            if (data.actorId == localActorId) return; // Ignore local actor

            if (remoteActors.ContainsKey(data.actorId))
            {
                Debug.LogWarning($"[SpacesBridge] Actor {data.actorId} already exists");
                return;
            }

            // Instantiate remote actor
            GameObject actorObj = null;
            if (remoteActorPrefab != null)
            {
                actorObj = Instantiate(remoteActorPrefab);
            }
            else
            {
                actorObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            }

            actorObj.name = $"RemoteActor_{data.actorId}";

            var remoteActor = new RemoteActor
            {
                actorId = data.actorId,
                gameObject = actorObj,
                displayName = data.displayName,
                avatarUrl = data.avatarUrl,
                targetPosition = data.position?.ToVector3() ?? Vector3.zero,
                targetRotation = Quaternion.Euler(data.rotation?.ToVector3() ?? Vector3.zero)
            };

            actorObj.transform.position = remoteActor.targetPosition;
            actorObj.transform.rotation = remoteActor.targetRotation;

            remoteActors[data.actorId] = remoteActor;
            OnActorJoined?.Invoke(data.actorId, remoteActor);

            Debug.Log($"[SpacesBridge] Actor joined: {data.actorId} ({data.displayName})");
        }

        private void HandleActorLeft(ActorLeftData data)
        {
            if (!remoteActors.TryGetValue(data.actorId, out var actor)) return;

            if (actor.gameObject != null)
            {
                Destroy(actor.gameObject);
            }

            remoteActors.Remove(data.actorId);
            OnActorLeft?.Invoke(data.actorId);

            Debug.Log($"[SpacesBridge] Actor left: {data.actorId}");
        }

        private void HandleActorUpdate(ActorUpdateData data)
        {
            if (data.actorId == localActorId) return;

            if (!remoteActors.TryGetValue(data.actorId, out var actor)) return;

            if (data.position != null)
                actor.targetPosition = data.position.ToVector3();
            if (data.rotation != null)
                actor.targetRotation = Quaternion.Euler(data.rotation.ToVector3());
            if (!string.IsNullOrEmpty(data.animation))
                actor.currentAnimation = data.animation;

            actor.isSpeaking = data.voice;
        }

        private void HandleObjectSpawned(ObjectSpawnedData data)
        {
            if (networkedObjects.ContainsKey(data.objectId))
            {
                Debug.LogWarning($"[SpacesBridge] Object {data.objectId} already exists");
                return;
            }

            // Instantiate object based on type
            GameObject obj = null;
            if (!string.IsNullOrEmpty(data.prefabId))
            {
                // TODO: Load prefab by ID from addressables/resources
                obj = defaultObjectPrefab != null ? Instantiate(defaultObjectPrefab) : GameObject.CreatePrimitive(PrimitiveType.Cube);
            }
            else
            {
                obj = defaultObjectPrefab != null ? Instantiate(defaultObjectPrefab) : GameObject.CreatePrimitive(PrimitiveType.Cube);
            }

            obj.name = $"NetworkedObject_{data.objectId}";
            obj.transform.position = data.position?.ToVector3() ?? Vector3.zero;
            obj.transform.rotation = Quaternion.Euler(data.rotation?.ToVector3() ?? Vector3.zero);
            obj.transform.localScale = data.scale?.ToVector3() ?? Vector3.one;

            var networkedObj = new NetworkedObject
            {
                objectId = data.objectId,
                objectType = data.objectType,
                ownerId = data.ownerId,
                gameObject = obj,
                state = data.state
            };

            networkedObjects[data.objectId] = networkedObj;
            OnObjectSpawned?.Invoke(data.objectId, networkedObj);

            Debug.Log($"[SpacesBridge] Object spawned: {data.objectId} ({data.objectType})");
        }

        private void HandleObjectDespawned(ObjectDespawnedData data)
        {
            if (!networkedObjects.TryGetValue(data.objectId, out var obj)) return;

            if (obj.gameObject != null)
            {
                Destroy(obj.gameObject);
            }

            networkedObjects.Remove(data.objectId);
            OnObjectDespawned?.Invoke(data.objectId);

            Debug.Log($"[SpacesBridge] Object despawned: {data.objectId}");
        }

        private void HandleObjectUpdate(ObjectUpdateData data)
        {
            if (!networkedObjects.TryGetValue(data.objectId, out var obj)) return;

            if (obj.gameObject != null)
            {
                if (data.position != null)
                    obj.gameObject.transform.position = data.position.ToVector3();
                if (data.rotation != null)
                    obj.gameObject.transform.rotation = Quaternion.Euler(data.rotation.ToVector3());
                if (data.scale != null)
                    obj.gameObject.transform.localScale = data.scale.ToVector3();
            }

            if (data.state != null)
            {
                obj.state = data.state;
            }

            if (!string.IsNullOrEmpty(data.ownerId))
            {
                obj.ownerId = data.ownerId;
            }
        }

        #endregion

        #region Internal

        private void SetState(BridgeState newState)
        {
            if (State == newState) return;
            State = newState;
            OnStateChanged?.Invoke(newState);
        }

        private float lastTransformSendTime = 0f;
        private const float TRANSFORM_SEND_INTERVAL = 0.05f; // 20 Hz

        private void SendLocalActorTransform()
        {
            if (Time.time - lastTransformSendTime < TRANSFORM_SEND_INTERVAL) return;
            lastTransformSendTime = Time.time;

            BridgeRaiseEvent.SendLocalTransform(new LocalTransformData
            {
                actorId = localActorId,
                position = new Vector3Data
                {
                    x = localActorTransform.position.x,
                    y = localActorTransform.position.y,
                    z = localActorTransform.position.z
                },
                rotation = new Vector3Data
                {
                    x = localActorTransform.eulerAngles.x,
                    y = localActorTransform.eulerAngles.y,
                    z = localActorTransform.eulerAngles.z
                }
            });
        }

        #endregion
    }

    #region State Enum

    public enum BridgeState
    {
        Disconnected,
        Connecting,
        Connected,
        Reconnecting
    }

    #endregion

    #region Remote Actor

    [Serializable]
    public class RemoteActor
    {
        public string actorId;
        public GameObject gameObject;
        public string displayName;
        public string avatarUrl;
        public Vector3 targetPosition;
        public Quaternion targetRotation;
        public string currentAnimation = "idle";
        public bool isSpeaking;

        public void Interpolate(float posSpeed, float rotSpeed, float deltaTime)
        {
            if (gameObject == null) return;

            gameObject.transform.position = Vector3.Lerp(
                gameObject.transform.position,
                targetPosition,
                posSpeed * deltaTime
            );

            gameObject.transform.rotation = Quaternion.Slerp(
                gameObject.transform.rotation,
                targetRotation,
                rotSpeed * deltaTime
            );
        }
    }

    #endregion

    #region Networked Object

    [Serializable]
    public class NetworkedObject
    {
        public string objectId;
        public string objectType;
        public string ownerId;
        public GameObject gameObject;
        public string state; // JSON string for custom state
    }

    #endregion
}

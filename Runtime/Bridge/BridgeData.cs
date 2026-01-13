using System;

namespace Spaces.React.Runtime.Bridge
{
    #region Incoming Data (React → Unity)

    [Serializable]
    public class BridgeConnectData
    {
        public string spaceId;
        public string instanceId;
        public string localActorId;
    }

    [Serializable]
    public class BridgeDisconnectData
    {
        public string reason;
    }

    [Serializable]
    public class ActorJoinedData
    {
        public string actorId;
        public string displayName;
        public string avatarUrl;
        public string role;
        public Vector3Data position;
        public Vector3Data rotation;
    }

    [Serializable]
    public class ActorLeftData
    {
        public string actorId;
    }

    [Serializable]
    public class ActorUpdateData
    {
        public string actorId;
        public Vector3Data position;
        public Vector3Data rotation;
        public string animation;
        public bool voice;
    }

    [Serializable]
    public class ObjectSpawnedData
    {
        public string objectId;
        public string objectType;
        public string ownerId;
        public string prefabId;
        public string glbUrl;
        public Vector3Data position;
        public Vector3Data rotation;
        public Vector3Data scale;
        public string state; // JSON string
    }

    [Serializable]
    public class ObjectDespawnedData
    {
        public string objectId;
    }

    [Serializable]
    public class ObjectUpdateData
    {
        public string objectId;
        public string ownerId;
        public Vector3Data position;
        public Vector3Data rotation;
        public Vector3Data scale;
        public string state; // JSON string
    }

    #endregion

    #region Outgoing Data (Unity → React)

    [Serializable]
    public class LocalTransformData
    {
        public string actorId;
        public Vector3Data position;
        public Vector3Data rotation;
    }

    [Serializable]
    public class LocalAnimationData
    {
        public string actorId;
        public string animation;
    }

    [Serializable]
    public class SpawnRequestData
    {
        public string objectType;
        public Vector3Data position;
        public Vector3Data rotation;
        public string prefabId;
    }

    [Serializable]
    public class DespawnRequestData
    {
        public string objectId;
    }

    [Serializable]
    public class OwnershipRequestData
    {
        public string objectId;
        public string requesterId;
    }

    [Serializable]
    public class CustomEventData
    {
        public string eventName;
        public string data; // JSON string
    }

    [Serializable]
    public class InputEventData
    {
        public string inputType; // click, key, etc.
        public string targetObjectId;
        public Vector3Data hitPoint;
        public string keyCode;
    }

    #endregion
}

using UnityEngine;

namespace Spaces.React.Runtime
{
    [System.Serializable]
    public class PortalTransformData
    {
        public string portalId;
        public Vector3Data position;
        public Vector3Data rotation;
        public Vector3Data scale;
    }

    [System.Serializable]
    public class Vector3Data
    {
        public float x;
        public float y;
        public float z;

        public Vector3 ToVector3()
        {
            Vector3 result = new Vector3(x, y, z);
            Debug.Log($"Converting Vector3Data to Vector3: ({x}, {y}, {z}) -> {result}");
            return result;
        }
    }
} 
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spaces.React.Runtime
{
    [Serializable]
    public class CatalogItemData
    {
        public string id;
        public string name;
        public string category;
        public string modelUrl;
        public string previewUrl;
        // Store position, rotation, and scale information
        public Vector3Json position;
        public Vector3Json rotation;
        public Vector3Json scale;
    }

    [Serializable]
    public class Vector3Json
    {
        public float x;
        public float y;
        public float z;

        public Vector3Json() { }

        public Vector3Json(Vector3 vector)
        {
            x = vector.x;
            y = vector.y;
            z = vector.z;
        }

        public Vector3 ToVector3()
        {
            return new Vector3(x, y, z);
        }
    }
} 
using UnityEngine;
using System;

namespace Spaces.React.Runtime
{
    [Serializable]
    public class SeatingHotspotData
    {
        public string hotspotId;
        public string glbUrl;
        public Vector3Data position;
        public Vector3Data rotation;
        public Vector3Data scale;
    }

    [Serializable]
    public class SeatingHotspotTransformData
    {
        public string hotspotId;
        public Vector3Data position;
        public Vector3Data rotation;
        public Vector3Data scale;
    }

    [Serializable]
    public class SeatingHotspotRegistrationData
    {
        public string hotspotId;
        public string position; // JSON string
        public string rotation; // JSON string
        public string scale;    // JSON string
        public string currentGlbUrl;
        public bool hasModel;
    }

    [Serializable]
    public class SeatingHotspotClickData
    {
        public string hotspotId;
    }
} 
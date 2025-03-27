using System;

namespace Spaces.React.Runtime
{
    [Serializable]
    public class HLSStreamData
    {
        public string identifier;    // Stream identifier (e.g., "LiveProjector") - replaces gameObjectName
        public string playerIndex;   // To identify which player (0, 1, etc.)
        public string streamUrl;     // The HLS stream URL to set
    }
} 
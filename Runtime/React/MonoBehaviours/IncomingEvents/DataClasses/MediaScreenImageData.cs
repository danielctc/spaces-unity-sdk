using System;

namespace Spaces.React.Runtime
{
    [Serializable]
    public class MediaScreenImageData
    {
        public string mediaScreenId;
        public string imageUrl;
        public string videoUrl;  // New field for video URL
        public string mediaType = "image"; // "image" or "video"
        public bool displayAsVideo = false;
        public long refreshTimestamp; // Timestamp to force refresh
    }
} 
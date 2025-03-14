using System;

namespace Spaces.React.Runtime
{
    [Serializable]
    public class MediaScreenThumbnailData
    {
        public string mediaScreenId;
        public string thumbnailUrl;
        public bool displayAsVideo = true;
        public long refreshTimestamp; // Timestamp to force refresh
    }
} 
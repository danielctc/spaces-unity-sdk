using System;

namespace Spaces.React.Runtime
{
    [Serializable]
    public class ForceUpdateMediaScreenData
    {
        public string mediaScreenId;
        public bool displayAsVideo = false;
        public long refreshTimestamp;
    }
} 
using System;

namespace Spaces.React.Runtime
{
    [Serializable]
    public class MediaScreenRegistrationData
    {
        public string mediaScreenId;
        public string position;
        public string rotation;
        public string scale;
        public string currentImageUrl;
        public bool hasImage;
    }
} 
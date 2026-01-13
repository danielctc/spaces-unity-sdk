using System;

namespace Spaces.React.Runtime
{
    /// <summary>
    /// Data class for placing a new video canvas
    /// </summary>
    [Serializable]
    public class VideoCanvasData
    {
        public string canvasId;
        public string videoUrl;
        public string videoType;      // youtube, vimeo, direct, hls
        public string aspectRatio;    // 16:9, 4:3, 1:1, 9:16
        public bool autoplay;
        public bool loop;
        public bool muted;
        public Vector3Data position;
        public Vector3Data rotation;
        public Vector3Data scale;
    }

    /// <summary>
    /// Data class for updating video canvas transform and settings
    /// </summary>
    [Serializable]
    public class VideoCanvasUpdateData
    {
        public string canvasId;
        public string videoUrl;
        public string videoType;
        public string aspectRatio;
        public bool autoplay;
        public bool loop;
        public bool muted;
        public Vector3Data position;
        public Vector3Data rotation;
        public Vector3Data scale;
    }

    /// <summary>
    /// Data class for deleting a video canvas
    /// </summary>
    [Serializable]
    public class VideoCanvasDeleteData
    {
        public string canvasId;
    }

    /// <summary>
    /// Data class for video canvas click events (Unity → React)
    /// </summary>
    [Serializable]
    public class VideoCanvasClickData
    {
        public string canvasId;
        public string videoUrl;
        public string position;  // JSON string of Vector3
    }

    /// <summary>
    /// Data class for video canvas registration (Unity → React)
    /// </summary>
    [Serializable]
    public class VideoCanvasRegistrationData
    {
        public string canvasId;
        public string position;   // JSON string
        public string rotation;   // JSON string
        public string scale;      // JSON string
        public string videoUrl;
        public bool hasVideo;
    }
}

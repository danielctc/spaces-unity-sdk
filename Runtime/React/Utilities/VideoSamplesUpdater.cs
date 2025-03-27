using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

namespace Spaces.React.Runtime
{
    public class VideoSamplesUpdater : MonoBehaviour
    {
        [Tooltip("The URL to force for testing")]
        public string testUrl = "https://stream.viloud.tv/hls/stream.m3u8";

        [Tooltip("Whether to force the test URL in play mode")]
        public bool forceTestUrlInPlayMode = true;

        [Tooltip("Whether to force the test URL in build")]
        public bool forceTestUrlInBuild = true;

        [Tooltip("Whether to log when URL is updated")]
        public bool logUrlUpdates = true;

        private bool hasInitialized = false;
        private bool isPlayerReady = false;

        private void OnEnable()
        {
            #if HISPLAYER_ENABLE
            if (!hasInitialized)
            {
                Initialize();
                hasInitialized = true;
            }
            ForceUrl();
            #endif
        }

        private void Update()
        {
            #if HISPLAYER_ENABLE
            if (isPlayerReady)
            {
                ForceUrl();
            }
            #endif
        }

        private void Initialize()
        {
            #if HISPLAYER_ENABLE
            try
            {
                // Get the HISPlayerManager type
                var managerType = System.Type.GetType("HISPlayer.HISPlayerManager");
                if (managerType == null)
                {
                    Debug.LogError("VideoSamplesUpdater: Could not find HISPlayerManager type!");
                    return;
                }

                // Get the StreamProperties type
                var propertiesType = System.Type.GetType("HISPlayer.StreamProperties");
                if (propertiesType == null)
                {
                    Debug.LogError("VideoSamplesUpdater: Could not find StreamProperties type!");
                    return;
                }

                // Create instance of StreamProperties
                var properties = System.Activator.CreateInstance(propertiesType);
                if (properties == null)
                {
                    Debug.LogError("VideoSamplesUpdater: Could not create StreamProperties instance!");
                    return;
                }

                // Set properties
                propertiesType.GetProperty("loop").SetValue(properties, true);
                propertiesType.GetProperty("autoTransition").SetValue(properties, false);
                propertiesType.GetProperty("autoPlay").SetValue(properties, true);
                propertiesType.GetProperty("EnableRendering").SetValue(properties, true);

                // Create array of one stream
                var streamArray = System.Array.CreateInstance(propertiesType, 1);
                streamArray.SetValue(properties, 0);

                // Get the videoSamples field
                var videoSamplesField = managerType.GetField("videoSamples", BindingFlags.Static | BindingFlags.NonPublic);
                if (videoSamplesField == null)
                {
                    Debug.LogError("VideoSamplesUpdater: Could not find videoSamples field!");
                    return;
                }

                // Set the videoSamples array
                videoSamplesField.SetValue(null, streamArray);

                // Check if player is ready
                var playerField = managerType.GetField("player", BindingFlags.Static | BindingFlags.NonPublic);
                if (playerField != null)
                {
                    var player = playerField.GetValue(null);
                    if (player != null)
                    {
                        isPlayerReady = true;
                        if (logUrlUpdates)
                        {
                            Debug.Log("VideoSamplesUpdater: Initialized successfully and player is ready");
                        }
                    }
                    else
                    {
                        Debug.LogWarning("VideoSamplesUpdater: Player is not set up yet, will retry in Update");
                    }
                }
                else
                {
                    Debug.LogWarning("VideoSamplesUpdater: Could not find player field, will retry in Update");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"VideoSamplesUpdater: Error during initialization: {e.Message}");
            }
            #endif
        }

        private void ForceUrl()
        {
            #if HISPLAYER_ENABLE
            try
            {
                // Get the HISPlayerManager type
                var managerType = System.Type.GetType("HISPlayer.HISPlayerManager");
                if (managerType == null)
                {
                    Debug.LogError("VideoSamplesUpdater: Could not find HISPlayerManager type!");
                    return;
                }

                // Check if player is ready
                var playerField = managerType.GetField("player", BindingFlags.Static | BindingFlags.NonPublic);
                if (playerField != null)
                {
                    var player = playerField.GetValue(null);
                    if (player == null)
                    {
                        if (!isPlayerReady)
                        {
                            Debug.LogWarning("VideoSamplesUpdater: Player is not set up yet, waiting...");
                            return;
                        }
                    }
                    else if (!isPlayerReady)
                    {
                        isPlayerReady = true;
                        if (logUrlUpdates)
                        {
                            Debug.Log("VideoSamplesUpdater: Player is now ready");
                        }
                    }
                }

                // Get the videoSamples field
                var videoSamplesField = managerType.GetField("videoSamples", BindingFlags.Static | BindingFlags.NonPublic);
                if (videoSamplesField == null)
                {
                    Debug.LogError("VideoSamplesUpdater: Could not find videoSamples field!");
                    return;
                }

                // Get the current videoSamples array
                var videoSamples = videoSamplesField.GetValue(null) as System.Array;
                if (videoSamples == null || videoSamples.Length == 0)
                {
                    Debug.LogError("VideoSamplesUpdater: No stream properties found!");
                    return;
                }

                // Get the StreamProperties type
                var propertiesType = videoSamples.GetType().GetElementType();
                if (propertiesType == null)
                {
                    Debug.LogError("VideoSamplesUpdater: Could not get StreamProperties type from array!");
                    return;
                }

                // Get the URL property
                var urlProperty = propertiesType.GetProperty("url");
                if (urlProperty == null)
                {
                    Debug.LogError("VideoSamplesUpdater: Could not find url property!");
                    return;
                }

                // Get current URL
                var currentUrl = urlProperty.GetValue(videoSamples.GetValue(0)) as string;

                // Determine if we should force the URL
                bool shouldForceUrl = false;
                #if UNITY_EDITOR
                shouldForceUrl = forceTestUrlInPlayMode && Application.isPlaying;
                #else
                shouldForceUrl = forceTestUrlInBuild;
                #endif

                // Update URL if needed
                if (shouldForceUrl && currentUrl != testUrl)
                {
                    urlProperty.SetValue(videoSamples.GetValue(0), testUrl);
                    if (logUrlUpdates)
                    {
                        Debug.Log($"VideoSamplesUpdater: Updated URL to {testUrl}");
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"VideoSamplesUpdater: Error during URL update: {e.Message}");
            }
            #endif
        }
    }
} 
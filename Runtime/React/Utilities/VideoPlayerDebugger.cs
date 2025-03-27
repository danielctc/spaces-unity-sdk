using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Spaces.React.Runtime
{
    /// <summary>
    /// Debug utility that displays the current state of the HISPlayerController URLs in the UI during runtime.
    /// </summary>
    [AddComponentMenu("Spaces/React/Video Player Debugger")]
    public class VideoPlayerDebugger : MonoBehaviour
    {
        [Tooltip("Reference to the VideoSamplesUpdater component")]
        public VideoSamplesUpdater videoSamplesUpdater;
        
        [Tooltip("Whether to log debug information")]
        public bool logDebugInfo = true;
        
        [Tooltip("How often to check player status (in seconds)")]
        public float checkInterval = 1.0f;
        
        private Coroutine statusCheckCoroutine;
        
        private void Start()
        {
            if (videoSamplesUpdater == null)
            {
                videoSamplesUpdater = GetComponent<VideoSamplesUpdater>();
                if (videoSamplesUpdater == null)
                {
                    Debug.LogError("VideoPlayerDebugger: No VideoSamplesUpdater found!");
                    return;
                }
            }
            
            StartStatusCheck();
        }
        
        private void OnDisable()
        {
            StopStatusCheck();
        }
        
        private void StartStatusCheck()
        {
            if (statusCheckCoroutine != null)
            {
                StopCoroutine(statusCheckCoroutine);
            }
            statusCheckCoroutine = StartCoroutine(CheckPlayerStatus());
        }
        
        private void StopStatusCheck()
        {
            if (statusCheckCoroutine != null)
            {
                StopCoroutine(statusCheckCoroutine);
                statusCheckCoroutine = null;
            }
        }
        
        private IEnumerator CheckPlayerStatus()
        {
            while (true)
            {
                #if HISPLAYER_ENABLE
                try
                {
                    // Get the HISPlayerManager type
                    var managerType = System.Type.GetType("HISPlayer.HISPlayerManager");
                    if (managerType == null)
                    {
                        Debug.LogError("VideoPlayerDebugger: Could not find HISPlayerManager type!");
                        yield break;
                    }
                    
                    // Get the videoSamples field
                    var videoSamplesField = managerType.GetField("videoSamples", BindingFlags.Static | BindingFlags.NonPublic);
                    if (videoSamplesField == null)
                    {
                        Debug.LogError("VideoPlayerDebugger: Could not find videoSamples field!");
                        yield break;
                    }
                    
                    // Get the current videoSamples array
                    var videoSamples = videoSamplesField.GetValue(null) as System.Array;
                    if (videoSamples == null || videoSamples.Length == 0)
                    {
                        Debug.LogError("VideoPlayerDebugger: No stream properties found!");
                        yield break;
                    }
                    
                    // Get the StreamProperties type
                    var propertiesType = videoSamples.GetType().GetElementType();
                    if (propertiesType == null)
                    {
                        Debug.LogError("VideoPlayerDebugger: Could not get StreamProperties type from array!");
                        yield break;
                    }
                    
                    // Get the URL property
                    var urlProperty = propertiesType.GetProperty("url");
                    if (urlProperty == null)
                    {
                        Debug.LogError("VideoPlayerDebugger: Could not find url property!");
                        yield break;
                    }
                    
                    // Get current URL
                    var currentUrl = urlProperty.GetValue(videoSamples.GetValue(0)) as string;
                    
                    if (logDebugInfo)
                    {
                        Debug.Log($"VideoPlayerDebugger: Current URL: {currentUrl}");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"VideoPlayerDebugger: Error checking player status: {e.Message}");
                }
                #endif
                
                yield return new WaitForSeconds(checkInterval);
            }
        }
        
        public void UpdateStreamUrl(string url)
        {
            #if HISPLAYER_ENABLE
            try
            {
                // Get the HISPlayerManager type
                var managerType = System.Type.GetType("HISPlayer.HISPlayerManager");
                if (managerType == null)
                {
                    Debug.LogError("VideoPlayerDebugger: Could not find HISPlayerManager type!");
                    return;
                }
                
                // Get the videoSamples field
                var videoSamplesField = managerType.GetField("videoSamples", BindingFlags.Static | BindingFlags.NonPublic);
                if (videoSamplesField == null)
                {
                    Debug.LogError("VideoPlayerDebugger: Could not find videoSamples field!");
                    return;
                }
                
                // Get the current videoSamples array
                var videoSamples = videoSamplesField.GetValue(null) as System.Array;
                if (videoSamples == null || videoSamples.Length == 0)
                {
                    Debug.LogError("VideoPlayerDebugger: No stream properties found!");
                    return;
                }
                
                // Get the StreamProperties type
                var propertiesType = videoSamples.GetType().GetElementType();
                if (propertiesType == null)
                {
                    Debug.LogError("VideoPlayerDebugger: Could not get StreamProperties type from array!");
                    return;
                }
                
                // Get the URL property
                var urlProperty = propertiesType.GetProperty("url");
                if (urlProperty == null)
                {
                    Debug.LogError("VideoPlayerDebugger: Could not find url property!");
                    return;
                }
                
                // Update the URL
                urlProperty.SetValue(videoSamples.GetValue(0), url);
                
                if (logDebugInfo)
                {
                    Debug.Log($"VideoPlayerDebugger: Updated URL to {url}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"VideoPlayerDebugger: Error updating stream URL: {e.Message}");
            }
            #endif
        }
    }
} 
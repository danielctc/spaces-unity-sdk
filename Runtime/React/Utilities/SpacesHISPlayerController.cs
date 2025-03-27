using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if HISPLAYER_ENABLE
using HISPlayerAPI;
using HISPlayer;
#endif

namespace Spaces.React.Runtime
{
    [AddComponentMenu("Spaces/React/Spaces HISPlayer Controller")]
    #if HISPLAYER_ENABLE
    public class SpacesHISPlayerController : HISPlayerManager
    #else
    public class SpacesHISPlayerController : MonoBehaviour
    #endif
    {
        #if HISPLAYER_ENABLE
        [Header("UI Helper")]
        public HISPlayerUIHelper HISPlayerUIHelper;

        [Header("Video Configuration")]
        [Tooltip("The URL to stream from")]
        public string streamUrl = "https://app.viloud.tv/hls/channel/67951f3e3286f823aa88edab9bf2713b.m3u8";

        [Tooltip("Whether to auto-play the video")]
        public bool autoPlay = true;

        [Tooltip("Whether to loop the video")]
        public bool loop = true;

        /// <summary>
        /// Determines if a stream is muted or not
        /// </summary>
        public List<bool> isMuted = new List<bool>() { false };

        /// <summary>
        /// Represents the number of the streams in the scene.
        /// It is initialized in function of multiStreamProperties.Count
        /// </summary>
        [HideInInspector]
        public int totalScreens = 0;

        /// <summary>
        /// Determines if a stream is playing or not
        /// </summary>
        [HideInInspector]
        public List<bool> isPlaying = new List<bool>();

        /// <summary>
        /// Determines if a stream is seeking.
        /// </summary>
        [HideInInspector]
        private List<bool> isSeeking = new List<bool>();

        /// <summary>
        /// Determines the current index of the video for each stream in the scene
        /// </summary>
        private List<int> videoIndex = new List<int>();

        /// <summary>
        /// Determines if a stream is ready to play or not
        /// </summary>
        private List<bool> isPlaybackReady = new List<bool>();

        /// <summary>
        /// Determines the current runtime platform
        /// </summary>
        private RuntimePlatform runtimePlatform = Application.platform;

        /// <summary>
        /// Determines if the quality has been set to 720 for the multistream performance.
        /// In the case of Windows this action is not needed becausue Windows doesn't support multi stream
        /// </summary>
        private List<bool> isQuality720 = new List<bool>();

        private string errorText = "";

        #region UNITY FUNCTIONS

        protected override void Awake()
        {
            if (runtimePlatform == RuntimePlatform.Android || runtimePlatform == RuntimePlatform.IPhonePlayer)
                Screen.orientation = ScreenOrientation.LandscapeLeft;

            base.Awake();
            
            // Create UI elements if needed
            if (HISPlayerUIHelper == null)
            {
                CreateUIHelper();
            }
            
            // Create and configure the stream properties
            ConfigureStreamProperties();
            
            // Set up the player
            SetUpPlayer();
            totalScreens = multiStreamProperties.Count;

            for (int i = 0; i < totalScreens; i++)
            {
                StreamProperties stream = multiStreamProperties[i];
                isPlaying.Add(stream.autoPlay);
                videoIndex.Add(i);
                isSeeking.Add(false);
                isPlaybackReady.Add(false);
                isQuality720.Add(false);

                SetVolume(i, isMuted[i] ? 0.0f : 1.0f);
                StartCoroutine(StartSomeValues(i));
                StartCoroutine(UpdateVideoPosition(i));
            }

            if (HISPlayerUIHelper != null)
            {
                HISPlayerUIHelper.InitUI(this);
            }
            else
            {
                Debug.LogWarning("SpacesHISPlayerController: No HISPlayerUIHelper assigned");
            }
        }
        
        private void ConfigureStreamProperties()
        {
            try
            {
                // Clear any existing stream properties
                if (multiStreamProperties != null)
                {
                    multiStreamProperties.Clear();
                }
                else
                {
                    Debug.LogError("SpacesHISPlayerController: multiStreamProperties is null!");
                    return;
                }
                
                // Create a new StreamProperties instance
                StreamProperties streamProperties = new StreamProperties();
                
                // Configure basic properties
                streamProperties.loop = loop;
                streamProperties.autoTransition = false;
                streamProperties.autoPlay = autoPlay;
                streamProperties.EnableRendering = true;
                
                // Create a render texture if needed
                if (GetComponent<Renderer>() && GetComponent<Renderer>().material != null)
                {
                    Material material = GetComponent<Renderer>().material;
                    
                    // Check if the material has a main texture and it's a render texture
                    if (material.mainTexture != null && material.mainTexture is RenderTexture)
                    {
                        RenderTexture renderTexture = (RenderTexture)material.mainTexture;
                        streamProperties.renderTexture = renderTexture;
                        Debug.Log($"SpacesHISPlayerController: Using existing render texture {renderTexture.width}x{renderTexture.height}");
                    }
                    else
                    {
                        // Create a new render texture
                        RenderTexture renderTexture = new RenderTexture(1920, 1080, 24);
                        renderTexture.Create();
                        
                        // Set it on the material
                        material.mainTexture = renderTexture;
                        
                        // Use it for streaming
                        streamProperties.renderTexture = renderTexture;
                        Debug.Log("SpacesHISPlayerController: Created new render texture 1920x1080");
                    }
                    
                    // Set material for rendering
                    streamProperties.material = material;
                    Debug.Log($"SpacesHISPlayerController: Using material {material.name}");
                }
                else
                {
                    // We need to find or create a material and render texture
                    
                    // First check if we're a parent of any renderers
                    Renderer childRenderer = GetComponentInChildren<Renderer>();
                    if (childRenderer != null && childRenderer.material != null)
                    {
                        Material material = childRenderer.material;
                        
                        // Create a render texture if needed
                        if (material.mainTexture == null || !(material.mainTexture is RenderTexture))
                        {
                            RenderTexture renderTexture = new RenderTexture(1920, 1080, 24);
                            renderTexture.Create();
                            material.mainTexture = renderTexture;
                        }
                        
                        RenderTexture rt = (RenderTexture)material.mainTexture;
                        
                        // Set up streaming properties
                        streamProperties.renderTexture = rt;
                        streamProperties.material = material;
                        
                        Debug.Log($"SpacesHISPlayerController: Using child renderer's material and render texture");
                    }
                    else
                    {
                        // Create a new quad with material for display
                        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                        quad.name = "VideoDisplay";
                        quad.transform.SetParent(transform);
                        quad.transform.localPosition = Vector3.zero;
                        quad.transform.localRotation = Quaternion.identity;
                        
                        // Set up 16:9 aspect ratio
                        quad.transform.localScale = new Vector3(16f, 9f, 1f);
                        
                        // Create material and render texture
                        Material material = new Material(Shader.Find("Unlit/Texture"));
                        RenderTexture renderTexture = new RenderTexture(1920, 1080, 24);
                        renderTexture.Create();
                        
                        material.mainTexture = renderTexture;
                        quad.GetComponent<Renderer>().material = material;
                        
                        // Set up streaming properties
                        streamProperties.renderTexture = renderTexture;
                        streamProperties.material = material;
                        
                        Debug.Log("SpacesHISPlayerController: Created new quad with material and render texture");
                    }
                }
                
                // Set render mode
                streamProperties.renderMode = HISPlayerRenderMode.Material;
                
                // Add URL
                if (string.IsNullOrEmpty(streamUrl))
                {
                    Debug.LogError("SpacesHISPlayerController: Stream URL is null or empty!");
                    return;
                }
                
                if (streamProperties.url == null)
                {
                    streamProperties.url = new List<string>();
                }
                
                streamProperties.url.Clear();
                streamProperties.url.Add(streamUrl);
                
                // Add to multiStreamProperties
                multiStreamProperties.Add(streamProperties);
                
                Debug.Log($"SpacesHISPlayerController: Configured stream with URL {streamUrl}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"SpacesHISPlayerController: Error configuring stream properties: {e.Message}\n{e.StackTrace}");
            }
        }

        private void OnApplicationFocus(bool focus)
        {
            for (int i = 0; i < totalScreens; i++)
            {
                if (!isPlaybackReady[i])
                    continue;

                if (focus)
                {
                    Play(i);
                }
                else
                {
                    Pause(i);
                }

                isPlaying[i] = focus;
                if (HISPlayerUIHelper != null)
                {
                    HISPlayerUIHelper.UpdatePlayPauseButton(i);
                }
            }
        }

        private void OnApplicationQuit()
        {
            Release();
        }

        #endregion

        #region PLAYBACK CONTROLLER

        public void OnSeekBegin(int playerIndex)
        {
            if (playerIndex < 0 || playerIndex >= totalScreens)
                return;

            isSeeking[playerIndex] = true;
        }

        public void OnSeekEnd(int playerIndex)
        {
            if (playerIndex < 0 || playerIndex >= totalScreens)
                return;
            
            if (HISPlayerUIHelper != null && HISPlayerUIHelper.seekBar != null && playerIndex < HISPlayerUIHelper.seekBar.Length)
            {
                long milliseconds = (long)HISPlayerUIHelper.seekBar[playerIndex].value;
                Seek(playerIndex, milliseconds);
            }
        }

        public void OnTogglePlayPause(int playerIndex)
        {
            if (playerIndex < 0 || playerIndex >= totalScreens)
                return;

            if (isPlaying[playerIndex])
            {
                Pause(playerIndex);
            }
            else
            {
                Play(playerIndex);
            }

            isPlaying[playerIndex] = !isPlaying[playerIndex];
            if (HISPlayerUIHelper != null)
            {
                HISPlayerUIHelper.UpdatePlayPauseButton(playerIndex);
            }
        }

        public void OnStop(int playerIndex)
        {
            isPlaying[playerIndex] = false;
            Stop(playerIndex);
            if (HISPlayerUIHelper != null)
            {
                HISPlayerUIHelper.UpdatePlayPauseButton(playerIndex);
            }
        }

        public void OnRestart(int playerIndex)
        {
            isPlaying[playerIndex] = true;
            Seek(playerIndex, 0);
            Play(playerIndex);

            if (HISPlayerUIHelper != null)
            {
                HISPlayerUIHelper.OnRestartTriggered(playerIndex);
                HISPlayerUIHelper.UpdateTotalTime(GetVideoDuration(playerIndex), playerIndex);
            }
        }

        public void OnToggleMute(int playerIndex)
        {
            if (playerIndex < 0 || playerIndex >= totalScreens)
                return;

            isMuted[playerIndex] = !isMuted[playerIndex];
            SetVolume(playerIndex, isMuted[playerIndex] ? 0.0f : 1.0f);
            if (HISPlayerUIHelper != null)
            {
                HISPlayerUIHelper.UpdateMuteButton(playerIndex);
            }
        }

        public void OnForward(int playerIndex)
        {
            if (playerIndex < 0 || playerIndex >= totalScreens)
                return;

            var currTime = GetVideoPosition(playerIndex);
            Seek(playerIndex, currTime + 10000);
        }

        public void OnBackward(int playerIndex)
        {
            if (playerIndex < 0 || playerIndex >= totalScreens)
                return;

            var currTime = GetVideoPosition(playerIndex);
            Seek(playerIndex, currTime - 10000);
        }

        public void SetStreamUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                Debug.LogError("SpacesHISPlayerController: Cannot set empty URL!");
                return;
            }
            
            streamUrl = url;
            Debug.Log($"SpacesHISPlayerController: Setting stream URL to {url}");
            
            int playerIndex = 0;
            isPlaybackReady[playerIndex] = false;
            isPlaying[playerIndex] = true;
            if (HISPlayerUIHelper != null)
            {
                HISPlayerUIHelper.ResetValues(playerIndex);
            }

            ResetUpdateVideoPosition(playerIndex);
            StartCoroutine(StartSomeValues(playerIndex));
            
            // Change the video content
            ChangeVideoContent(playerIndex, url);
        }

        public void OnChangeSpeedRate(int playerIndex)
        {
            float currentSpeed = GetPlaybackSpeedRate(playerIndex);
            float newSpeed = 1.0f;
            switch (currentSpeed)
            {
                case 1.0f:
                    newSpeed = 1.25f;
                    break;
                case 1.25f:
                    newSpeed = 1.5f;
                    break;
                case 1.5f:
                    newSpeed = 2.0f;
                    break;
                case 2.0f:
                    newSpeed = 8.0f;
                    break;
                case 8.0f:
                    newSpeed = 1.0f;
                    break;
                default:
                    break;
            }

            SetPlaybackSpeedRate(playerIndex, newSpeed);
            if (HISPlayerUIHelper != null)
            {
                HISPlayerUIHelper.UpdateSpeedRateText(playerIndex, newSpeed);
            }
        }

        #endregion

        #region MISC

        IEnumerator StartSomeValues(int playerIndex)
        {
            yield return new WaitUntil(() => isPlaybackReady[playerIndex]);

            if (HISPlayerUIHelper != null)
            {
                HISPlayerUIHelper.UpdateTotalTime(GetVideoDuration(playerIndex), playerIndex);
                HISPlayerUIHelper.UpdateMuteButton(playerIndex);
                SetVolume(playerIndex, isMuted[playerIndex] ? 0.0f : 1.0f);

                SetPlaybackSpeedRate(playerIndex, 1.0f);
                HISPlayerUIHelper.UpdateSpeedRateText(playerIndex, 1.0f);
            }
        }

        IEnumerator UpdateVideoPosition(int playerIndex)
        {
            yield return new WaitUntil(() => isPlaybackReady[playerIndex]);

            while (isPlaybackReady[playerIndex])
            {
                if (HISPlayerUIHelper != null)
                {
                    float ms = 0;
                    if (!isSeeking[playerIndex])
                    {
                        ms = GetVideoPosition(playerIndex);
                    }
                    else
                    {
                        if (playerIndex < HISPlayerUIHelper.seekBar.Length)
                        {
                            ms = HISPlayerUIHelper.seekBar[playerIndex].value;
                        }
                    }

                    HISPlayerUIHelper.UpdateVideoPosition((long)ms, playerIndex);
                }

                yield return null;
            }
        }

        private void ResetUpdateVideoPosition(int playerIndex)
        {
            StopCoroutine(UpdateVideoPosition(playerIndex));
            StartCoroutine(UpdateVideoPosition(playerIndex));
        }

        public void ReleasePlayer()
        {
            Release();
        }

        public void SetAllPlaybacksAt720(int playerIndex)
        {
            if (isQuality720[playerIndex])
                return;

            var tracks = GetTracks(playerIndex);
            if (tracks == null)
            {
                return;
            }

            int i = 0;
            while (i < tracks.Length && !isQuality720[playerIndex])
            {
                var track = tracks[i];
                if (track.width == 1280 && track.height == 720)
                {
                    SelectTrack(playerIndex, i);
                    isQuality720[playerIndex] = true;
                }

                i++;
            }
        }

        #endregion

        #region HISPLAYER EVENTS

        protected override void EventPlaybackPlay(HISPlayerEventInfo eventInfo)
        {
            base.EventPlaybackPlay(eventInfo);
            int playerIndex = eventInfo.playerIndex;
            isPlaying[playerIndex] = true;

            if (HISPlayerUIHelper != null)
            {
                HISPlayerUIHelper.UpdateErrorText(playerIndex, "");
                HISPlayerUIHelper.UpdatePlayPauseButton(playerIndex);
            }
        }

        protected override void EventPlaybackReady(HISPlayerEventInfo eventInfo)
        {
            base.EventPlaybackReady(eventInfo);
            int playerIndex = eventInfo.playerIndex;

            isPlaybackReady[playerIndex] = true;
            if (HISPlayerUIHelper != null)
            {
                HISPlayerUIHelper.UpdateTotalTime(GetVideoDuration(playerIndex), playerIndex);
            }

            if (runtimePlatform != RuntimePlatform.Android)
                SetAllPlaybacksAt720(playerIndex);
        }

        protected override void EventEndOfPlaylist(HISPlayerEventInfo eventInfo)
        {
            base.EventEndOfPlaylist(eventInfo);

            int playerIndex = eventInfo.playerIndex;

            if (multiStreamProperties[playerIndex].loop)
                return;

            isPlaybackReady[playerIndex] = false;
            isPlaying[playerIndex] = false;

            if (HISPlayerUIHelper != null)
            {
                HISPlayerUIHelper.ResetValues(playerIndex, restart: true);
            }

            StartCoroutine(StartSomeValues(playerIndex));
            ChangeVideoContent(playerIndex, streamUrl);
        }

        protected override void EventPlaybackSeek(HISPlayerEventInfo eventInfo)
        {
            base.EventPlaybackSeek(eventInfo);
            isSeeking[eventInfo.playerIndex] = false;
        }

        protected override void EventVideoSizeChange(HISPlayerEventInfo eventInfo)
        {
            base.EventVideoSizeChange(eventInfo);
            if (runtimePlatform == RuntimePlatform.Android)
                SetAllPlaybacksAt720(eventInfo.playerIndex);
        }

        protected override void EventOnTrackChange(HISPlayerEventInfo eventInfo)
        {
            base.EventOnTrackChange(eventInfo);
        }

        protected override void ErrorInfo(HISPlayerErrorInfo errorInfo)
        {
            base.ErrorInfo(errorInfo);
            errorText = errorInfo.stringInfo;
            Debug.LogError($"SpacesHISPlayerController Error: {errorText}");
            
            if (HISPlayerUIHelper != null)
            {
                if (errorInfo.errorType == HISPlayerError.HISPLAYER_ERROR_PLAYBACK_DURATION_LIMIT_REACHED)
                {
                    HISPlayerUIHelper.UpdateErrorText(errorInfo.playerIndex, errorText);
                    return;
                }

                HISPlayerUIHelper.UpdateGeneralErrorText(errorText);
            }
        }

        protected override void EventPlaybackStop(HISPlayerEventInfo eventInfo)
        {
            base.EventPlaybackStop(eventInfo);
            int playerIndex = eventInfo.playerIndex;
            isPlaying[playerIndex] = false;
            
            if (HISPlayerUIHelper != null)
            {
                HISPlayerUIHelper.UpdatePlayPauseButton(playerIndex);
            }
        }

        #endregion

        private void CreateUIHelper()
        {
            try
            {
                // Create a UI GameObject
                GameObject uiObject = new GameObject("HISPlayerUI");
                uiObject.transform.SetParent(transform);
                
                // Add a Canvas component
                Canvas canvas = uiObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.WorldSpace;
                
                // Add a Canvas Scaler component
                UnityEngine.UI.CanvasScaler canvasScaler = uiObject.AddComponent<UnityEngine.UI.CanvasScaler>();
                canvasScaler.dynamicPixelsPerUnit = 100f;
                
                // Add a Graphic Raycaster component
                uiObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                
                // Position the canvas in front of the video
                uiObject.transform.localPosition = new Vector3(0f, 0f, -0.01f);
                uiObject.transform.localRotation = Quaternion.identity;
                uiObject.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                
                // Create the HISPlayerUIHelper component
                HISPlayerUIHelper = uiObject.AddComponent<HISPlayerUIHelper>();
                
                // Create UI elements
                CreateUIElements(uiObject);
                
                Debug.Log("SpacesHISPlayerController: Created UI helper and elements");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"SpacesHISPlayerController: Error creating UI helper: {e.Message}");
            }
        }

        private void CreateUIElements(GameObject parent)
        {
            // Create a list for UI containers
            HISPlayerUIHelper.UI = new List<GameObject>();
            
            // Create a UI container
            GameObject container = new GameObject("UIContainer");
            container.transform.SetParent(parent.transform, false);
            UnityEngine.UI.Image containerImage = container.AddComponent<UnityEngine.UI.Image>();
            containerImage.color = new Color(0.1f, 0.1f, 0.1f, 0.7f);
            UnityEngine.UI.RectTransform containerRect = container.GetComponent<UnityEngine.UI.RectTransform>();
            containerRect.anchorMin = new Vector2(0, 0);
            containerRect.anchorMax = new Vector2(1, 0.2f);
            containerRect.offsetMin = Vector2.zero;
            containerRect.offsetMax = Vector2.zero;
            
            // Add the container to the UI list
            HISPlayerUIHelper.UI.Add(container);
            
            // Create play/pause button
            GameObject playPauseButton = CreateButton(container.transform, "PlayPauseButton", new Vector2(0.1f, 0.5f), new Vector2(40, 40));
            
            // Create mute button
            GameObject muteButton = CreateButton(container.transform, "MuteButton", new Vector2(0.2f, 0.5f), new Vector2(40, 40));
            
            // Create restart button
            GameObject restartButton = CreateButton(container.transform, "RestartButton", new Vector2(0.3f, 0.5f), new Vector2(40, 40));
            restartButton.SetActive(false);
            
            // Create seek bar
            GameObject seekBarObj = new GameObject("SeekBar");
            seekBarObj.transform.SetParent(container.transform, false);
            UnityEngine.UI.Slider seekBar = seekBarObj.AddComponent<UnityEngine.UI.Slider>();
            UnityEngine.UI.RectTransform seekBarRect = seekBarObj.GetComponent<UnityEngine.UI.RectTransform>();
            seekBarRect.anchorMin = new Vector2(0.4f, 0.5f);
            seekBarRect.anchorMax = new Vector2(0.9f, 0.5f);
            seekBarRect.anchoredPosition = Vector2.zero;
            seekBarRect.sizeDelta = new Vector2(0, 20);
            
            // Create slider components
            GameObject background = new GameObject("Background");
            background.transform.SetParent(seekBarObj.transform, false);
            UnityEngine.UI.Image backgroundImage = background.AddComponent<UnityEngine.UI.Image>();
            backgroundImage.color = Color.gray;
            UnityEngine.UI.RectTransform backgroundRect = background.GetComponent<UnityEngine.UI.RectTransform>();
            backgroundRect.anchorMin = Vector2.zero;
            backgroundRect.anchorMax = Vector2.one;
            backgroundRect.sizeDelta = Vector2.zero;
            
            GameObject fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(seekBarObj.transform, false);
            UnityEngine.UI.RectTransform fillAreaRect = fillArea.AddComponent<UnityEngine.UI.RectTransform>();
            fillAreaRect.anchorMin = new Vector2(0, 0.5f);
            fillAreaRect.anchorMax = new Vector2(1, 0.5f);
            fillAreaRect.offsetMin = new Vector2(5, -5);
            fillAreaRect.offsetMax = new Vector2(-5, 5);
            
            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            UnityEngine.UI.Image fillImage = fill.AddComponent<UnityEngine.UI.Image>();
            fillImage.color = Color.white;
            UnityEngine.UI.RectTransform fillRect = fill.GetComponent<UnityEngine.UI.RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = new Vector2(0.5f, 1);
            fillRect.sizeDelta = Vector2.zero;
            
            GameObject handleArea = new GameObject("Handle Slide Area");
            handleArea.transform.SetParent(seekBarObj.transform, false);
            UnityEngine.UI.RectTransform handleAreaRect = handleArea.AddComponent<UnityEngine.UI.RectTransform>();
            handleAreaRect.anchorMin = new Vector2(0, 0.5f);
            handleAreaRect.anchorMax = new Vector2(1, 0.5f);
            handleAreaRect.offsetMin = new Vector2(10, -10);
            handleAreaRect.offsetMax = new Vector2(-10, 10);
            
            GameObject handle = new GameObject("Handle");
            handle.transform.SetParent(handleArea.transform, false);
            UnityEngine.UI.Image handleImage = handle.AddComponent<UnityEngine.UI.Image>();
            handleImage.color = Color.white;
            UnityEngine.UI.RectTransform handleRect = handle.GetComponent<UnityEngine.UI.RectTransform>();
            handleRect.anchorMin = new Vector2(0.5f, 0.5f);
            handleRect.anchorMax = new Vector2(0.5f, 0.5f);
            handleRect.sizeDelta = new Vector2(20, 20);
            
            // Configure the slider
            seekBar.fillRect = fillRect;
            seekBar.handleRect = handleRect;
            seekBar.targetGraphic = handleImage;
            seekBar.direction = UnityEngine.UI.Slider.Direction.LeftToRight;
            seekBar.minValue = 0;
            seekBar.maxValue = 100;
            seekBar.value = 0;
            
            // Create text for current time
            GameObject currTimeTextObj = new GameObject("CurrentTimeText");
            currTimeTextObj.transform.SetParent(container.transform, false);
            TMPro.TextMeshProUGUI currTimeText = currTimeTextObj.AddComponent<TMPro.TextMeshProUGUI>();
            currTimeText.text = "0:00:00";
            currTimeText.fontSize = 14;
            currTimeText.alignment = TMPro.TextAlignmentOptions.Center;
            UnityEngine.UI.RectTransform currTimeTextRect = currTimeTextObj.GetComponent<UnityEngine.UI.RectTransform>();
            currTimeTextRect.anchorMin = new Vector2(0.35f, 0.2f);
            currTimeTextRect.anchorMax = new Vector2(0.45f, 0.4f);
            currTimeTextRect.sizeDelta = Vector2.zero;
            
            // Create text for total time
            GameObject totalTimeTextObj = new GameObject("TotalTimeText");
            totalTimeTextObj.transform.SetParent(container.transform, false);
            TMPro.TextMeshProUGUI totalTimeText = totalTimeTextObj.AddComponent<TMPro.TextMeshProUGUI>();
            totalTimeText.text = "Loading";
            totalTimeText.fontSize = 14;
            totalTimeText.alignment = TMPro.TextAlignmentOptions.Center;
            UnityEngine.UI.RectTransform totalTimeTextRect = totalTimeTextObj.GetComponent<UnityEngine.UI.RectTransform>();
            totalTimeTextRect.anchorMin = new Vector2(0.85f, 0.2f);
            totalTimeTextRect.anchorMax = new Vector2(0.95f, 0.4f);
            totalTimeTextRect.sizeDelta = Vector2.zero;
            
            // Create text for speed rate
            GameObject speedRateTextObj = new GameObject("SpeedRateText");
            speedRateTextObj.transform.SetParent(container.transform, false);
            TMPro.TextMeshProUGUI speedRateText = speedRateTextObj.AddComponent<TMPro.TextMeshProUGUI>();
            speedRateText.text = "x1.0";
            speedRateText.fontSize = 14;
            speedRateText.alignment = TMPro.TextAlignmentOptions.Center;
            UnityEngine.UI.RectTransform speedRateTextRect = speedRateTextObj.GetComponent<UnityEngine.UI.RectTransform>();
            speedRateTextRect.anchorMin = new Vector2(0.91f, 0.6f);
            speedRateTextRect.anchorMax = new Vector2(0.99f, 0.8f);
            speedRateTextRect.sizeDelta = Vector2.zero;
            
            // Create text for error messages
            GameObject errorTextObj = new GameObject("ErrorText");
            errorTextObj.transform.SetParent(parent.transform, false);
            TMPro.TextMeshProUGUI errorText = errorTextObj.AddComponent<TMPro.TextMeshProUGUI>();
            errorText.text = "";
            errorText.fontSize = 16;
            errorText.color = Color.red;
            errorText.alignment = TMPro.TextAlignmentOptions.Center;
            UnityEngine.UI.RectTransform errorTextRect = errorTextObj.GetComponent<UnityEngine.UI.RectTransform>();
            errorTextRect.anchorMin = new Vector2(0.1f, 0.7f);
            errorTextRect.anchorMax = new Vector2(0.9f, 0.9f);
            errorTextRect.sizeDelta = Vector2.zero;
            
            // Create text for general error messages
            GameObject generalErrorTextObj = new GameObject("GeneralErrorText");
            generalErrorTextObj.transform.SetParent(parent.transform, false);
            TMPro.TextMeshProUGUI generalErrorText = generalErrorTextObj.AddComponent<TMPro.TextMeshProUGUI>();
            generalErrorText.text = "";
            generalErrorText.fontSize = 16;
            generalErrorText.color = Color.red;
            generalErrorText.alignment = TMPro.TextAlignmentOptions.Center;
            UnityEngine.UI.RectTransform generalErrorTextRect = generalErrorTextObj.GetComponent<UnityEngine.UI.RectTransform>();
            generalErrorTextRect.anchorMin = new Vector2(0.1f, 0.5f);
            generalErrorTextRect.anchorMax = new Vector2(0.9f, 0.7f);
            generalErrorTextRect.sizeDelta = Vector2.zero;
            generalErrorTextObj.SetActive(false);
            
            // Set up arrays for UI elements
            HISPlayerUIHelper.playPauseButton = new UnityEngine.UI.Button[] { playPauseButton.GetComponent<UnityEngine.UI.Button>() };
            HISPlayerUIHelper.muteButton = new UnityEngine.UI.Button[] { muteButton.GetComponent<UnityEngine.UI.Button>() };
            HISPlayerUIHelper.restartButton = new UnityEngine.UI.Button[] { restartButton.GetComponent<UnityEngine.UI.Button>() };
            HISPlayerUIHelper.seekBar = new UnityEngine.UI.Slider[] { seekBar };
            HISPlayerUIHelper.currTimeText = new TMPro.TextMeshProUGUI[] { currTimeText };
            HISPlayerUIHelper.totalTimeText = new TMPro.TextMeshProUGUI[] { totalTimeText };
            HISPlayerUIHelper.speedRateText = new TMPro.TextMeshProUGUI[] { speedRateText };
            HISPlayerUIHelper.errorText = new TMPro.TextMeshProUGUI[] { errorText };
            HISPlayerUIHelper.generalErrorText = generalErrorText;
            
            // Set up button sprites
            CreateDefaultSprites();
            
            // Set initial sprites
            playPauseButton.GetComponent<UnityEngine.UI.Image>().sprite = HISPlayerUIHelper.playSprite;
            muteButton.GetComponent<UnityEngine.UI.Image>().sprite = HISPlayerUIHelper.unmuteSprite;
            restartButton.GetComponent<UnityEngine.UI.Image>().sprite = HISPlayerUIHelper.restartSprite;
            
            // Set up button callbacks
            playPauseButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => OnTogglePlayPause(0));
            muteButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => OnToggleMute(0));
            restartButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => OnRestart(0));
            
            // Set up seek bar events
            UnityEngine.EventSystems.EventTrigger trigger = seekBarObj.AddComponent<UnityEngine.EventSystems.EventTrigger>();
            
            var pointerDown = new UnityEngine.EventSystems.EventTrigger.Entry();
            pointerDown.eventID = UnityEngine.EventSystems.EventTriggerType.PointerDown;
            pointerDown.callback.AddListener((eventData) => { OnSeekBegin(0); });
            trigger.triggers.Add(pointerDown);
            
            var pointerUp = new UnityEngine.EventSystems.EventTrigger.Entry();
            pointerUp.eventID = UnityEngine.EventSystems.EventTriggerType.PointerUp;
            pointerUp.callback.AddListener((eventData) => { OnSeekEnd(0); });
            trigger.triggers.Add(pointerUp);
        }

        private GameObject CreateButton(Transform parent, string name, Vector2 anchorPosition, Vector2 size)
        {
            GameObject button = new GameObject(name);
            button.transform.SetParent(parent, false);
            
            UnityEngine.UI.Image image = button.AddComponent<UnityEngine.UI.Image>();
            image.color = Color.white;
            
            UnityEngine.UI.Button buttonComponent = button.AddComponent<UnityEngine.UI.Button>();
            UnityEngine.UI.ColorBlock colors = buttonComponent.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f);
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f);
            buttonComponent.colors = colors;
            
            UnityEngine.UI.RectTransform rectTransform = button.GetComponent<UnityEngine.UI.RectTransform>();
            rectTransform.anchorMin = new Vector2(anchorPosition.x - 0.05f, anchorPosition.y - 0.5f);
            rectTransform.anchorMax = new Vector2(anchorPosition.x + 0.05f, anchorPosition.y + 0.5f);
            rectTransform.sizeDelta = size;
            rectTransform.anchoredPosition = Vector2.zero;
            
            return button;
        }

        private void CreateDefaultSprites()
        {
            // Play sprite - triangle
            Texture2D playTexture = new Texture2D(32, 32, TextureFormat.RGBA32, false);
            Color[] playColors = new Color[32 * 32];
            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 32; x++)
                {
                    int index = y * 32 + x;
                    if (x >= 8 && x <= 24 && y >= 8 && y <= 24)
                    {
                        // Create a triangle
                        if (x <= 8 + (y - 8) * 0.8f)
                        {
                            playColors[index] = Color.white;
                        }
                        else
                        {
                            playColors[index] = new Color(1, 1, 1, 0);
                        }
                    }
                    else
                    {
                        playColors[index] = new Color(1, 1, 1, 0);
                    }
                }
            }
            playTexture.SetPixels(playColors);
            playTexture.Apply();
            HISPlayerUIHelper.playSprite = Sprite.Create(playTexture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
            
            // Pause sprite - two vertical bars
            Texture2D pauseTexture = new Texture2D(32, 32, TextureFormat.RGBA32, false);
            Color[] pauseColors = new Color[32 * 32];
            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 32; x++)
                {
                    int index = y * 32 + x;
                    if (y >= 8 && y <= 24)
                    {
                        if ((x >= 10 && x <= 14) || (x >= 18 && x <= 22))
                        {
                            pauseColors[index] = Color.white;
                        }
                        else
                        {
                            pauseColors[index] = new Color(1, 1, 1, 0);
                        }
                    }
                    else
                    {
                        pauseColors[index] = new Color(1, 1, 1, 0);
                    }
                }
            }
            pauseTexture.SetPixels(pauseColors);
            pauseTexture.Apply();
            HISPlayerUIHelper.pauseSprite = Sprite.Create(pauseTexture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
            
            // Mute sprite - speaker with X
            Texture2D muteTexture = new Texture2D(32, 32, TextureFormat.RGBA32, false);
            Color[] muteColors = new Color[32 * 32];
            for (int i = 0; i < muteColors.Length; i++)
            {
                muteColors[i] = new Color(1, 1, 1, 0);
            }
            // Draw a simple speaker icon with X
            for (int y = 10; y < 22; y++)
            {
                for (int x = 10; x < 22; x++)
                {
                    int index = y * 32 + x;
                    muteColors[index] = Color.white;
                }
            }
            muteTexture.SetPixels(muteColors);
            muteTexture.Apply();
            HISPlayerUIHelper.muteSprite = Sprite.Create(muteTexture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
            
            // Unmute sprite - speaker
            Texture2D unmuteTexture = new Texture2D(32, 32, TextureFormat.RGBA32, false);
            Color[] unmuteColors = new Color[32 * 32];
            for (int i = 0; i < unmuteColors.Length; i++)
            {
                unmuteColors[i] = new Color(1, 1, 1, 0);
            }
            // Draw a simple speaker icon
            for (int y = 10; y < 22; y++)
            {
                for (int x = 10; x < 22; x++)
                {
                    int index = y * 32 + x;
                    unmuteColors[index] = Color.white;
                }
            }
            unmuteTexture.SetPixels(unmuteColors);
            unmuteTexture.Apply();
            HISPlayerUIHelper.unmuteSprite = Sprite.Create(unmuteTexture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
            
            // Restart sprite - circular arrow
            Texture2D restartTexture = new Texture2D(32, 32, TextureFormat.RGBA32, false);
            Color[] restartColors = new Color[32 * 32];
            for (int i = 0; i < restartColors.Length; i++)
            {
                restartColors[i] = new Color(1, 1, 1, 0);
            }
            // Draw a simple circular arrow
            for (int y = 8; y < 24; y++)
            {
                for (int x = 8; x < 24; x++)
                {
                    int dx = x - 16;
                    int dy = y - 16;
                    int distSq = dx * dx + dy * dy;
                    if (distSq > 36 && distSq < 64)
                    {
                        restartColors[y * 32 + x] = Color.white;
                    }
                }
            }
            restartTexture.SetPixels(restartColors);
            restartTexture.Apply();
            HISPlayerUIHelper.restartSprite = Sprite.Create(restartTexture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
        }
        #else
        [Tooltip("The URL to stream from")]
        public string streamUrl = "https://app.viloud.tv/hls/channel/67951f3e3286f823aa88edab9bf2713b.m3u8";
        
        private void Awake()
        {
            Debug.LogError("SpacesHISPlayerController: HISPLAYER_ENABLE is not defined! Video streaming will not work. Add HISPLAYER_ENABLE to Scripting Define Symbols in Player Settings.");
        }
        #endif
    }
} 
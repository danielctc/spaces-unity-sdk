using UnityEngine;
using System.Runtime.InteropServices;
using Spaces.React.Runtime; 
using System;

public static class ReactRaiseEvent
{
    [DllImport("__Internal")]
    private static extern void JsFirstSceneLoaded();

    public static void FirstSceneLoaded()
    {
        Debug.Log("Unity: Start Raising FirstSceneLoaded event");
#if !UNITY_EDITOR && UNITY_WEBGL
        JsFirstSceneLoaded();
#endif
        Debug.Log("Unity: End Raising FirstSceneLoaded event");
    }

    [DllImport("__Internal")]
    private static extern void JsRequestUserForUnity();

    public static void RequestUserForUnity()
    {
        Debug.Log("Unity: Start Raising RequestUserForUnity event");
#if !UNITY_EDITOR && UNITY_WEBGL
        JsRequestUserForUnity();
#endif
        Debug.Log("Unity: End Raising RequestUserForUnity event");
    }

    [DllImport("__Internal")]
    private static extern void JsStoreUserData(string userDataJson);

    public static void StoreUserData(FirebaseUserData firebaseUserData)
    {
        string userDataJson = JsonUtility.ToJson(firebaseUserData);
        Debug.Log("Unity: Raising StoreUserData event: " + userDataJson);
#if !UNITY_EDITOR && UNITY_WEBGL
        JsStoreUserData(userDataJson);
#endif
    }

    [DllImport("__Internal")]
    private static extern void JsPlayVideo(string eventDataJson);

    public static void PlayVideo(PlayVideoData eventData)
    {
        string eventDataJson = JsonUtility.ToJson(eventData);
        Debug.Log("Unity: Raising PlayVideo event: " + eventDataJson);
#if !UNITY_EDITOR && UNITY_WEBGL
        JsPlayVideo(eventDataJson);
#endif
    }

    [DllImport("__Internal")]
    private static extern void JsHelloFromUnity(string eventDataJson);

    public static void HelloFromUnity(HelloFromUnityData eventData)
    {
        string eventDataJson = JsonUtility.ToJson(eventData);
        Debug.Log("Unity: Raising HelloFromUnity event");
#if !UNITY_EDITOR && UNITY_WEBGL
        JsHelloFromUnity(eventDataJson);
#endif
    }

    // New addition for player instantiation
    [DllImport("__Internal")]
    private static extern void JsPlayerInstantiated();

    public static void PlayerInstantiated()
    {
        Debug.Log("Unity: Start Raising PlayerInstantiated event");
#if !UNITY_EDITOR && UNITY_WEBGL
        JsPlayerInstantiated();
#endif
        Debug.Log("Unity: End Raising PlayerInstantiated event");
    }

    // Existing nameplate modal method
    [DllImport("__Internal")]
    private static extern void JsOpenNameplateModal(string nameplateDataJson);

    public static void OpenNameplateModal(NameplateClickData nameplateData)
    {
        string nameplateDataJson = JsonUtility.ToJson(nameplateData);
        Debug.Log("Unity: Raising OpenNameplateModal event: " + nameplateDataJson);
#if !UNITY_EDITOR && UNITY_WEBGL
        JsOpenNameplateModal(nameplateDataJson);
#endif
    }
    
    // NEW: Update for sending the player list to React.
    [DllImport("__Internal")]
    private static extern void JsUpdatePlayerList(string playerListJson);

    public static void UpdatePlayerList(string jsonData)
    {
        Debug.Log("Unity: Raising UpdatePlayerList event: " + jsonData);
#if !UNITY_EDITOR && UNITY_WEBGL
        JsUpdatePlayerList(jsonData);
#endif
    }

    // Add this method to the ReactRaiseEvent class
    public static void SendKeyboardCaptureRequest(KeyboardCaptureRequestData data)
    {
        string jsonData = JsonUtility.ToJson(data);
        Debug.Log($"Unity: Sending keyboard capture request: {jsonData}");

    #if UNITY_WEBGL && !UNITY_EDITOR
        JsKeyboardCaptureRequest(jsonData);
    #else
        Debug.Log($"[SendKeyboardCaptureRequest] Would send to React: {jsonData}");
    #endif
    }

    [DllImport("__Internal")]
    private static extern void JsKeyboardCaptureRequest(string jsonData);

    // Add new method for HLS Stream status updates
    [DllImport("__Internal")]
    private static extern void JsSetHLSStream(string jsonData);

    public static void SendHLSStreamStatus(string identifier, string playerIndex, bool isReady, bool isPlaying)
    {
        // Create anonymous type for JSON serialization
        var statusData = new { 
            identifier = identifier,
            playerIndex = playerIndex, 
            isReady = isReady, 
            isPlaying = isPlaying 
        };
        
        string jsonData = JsonUtility.ToJson(statusData);
        Debug.Log($"Unity: Sending HLSStreamStatus to React with data: {jsonData}");
        
        #if !UNITY_EDITOR && UNITY_WEBGL
        JsSetHLSStream(jsonData);
        #else
        Debug.Log("Not running in WebGL build, JavaScript function not called.");
        #endif
    }

    [DllImport("__Internal")]
    private static extern void JsPlacePrefab(string data);

    public static void PlacePrefab(PrefabPlacementData data)
    {
        string jsonData = JsonUtility.ToJson(data);
        Debug.Log($"Unity: Sending PlacePrefab event with data: {jsonData}");
        
        #if UNITY_WEBGL && !UNITY_EDITOR
        JsPlacePrefab(jsonData);
        #endif
    }

    [DllImport("__Internal")]
    private static extern void JsKickPlayerResult(string jsonData);

    public static void SendKickPlayerResult(bool success, string playerName, string playerUid, string errorMessage = "")
    {
        KickPlayerResultData resultData = new KickPlayerResultData
        {
            success = success,
            playerName = playerName,
            playerUid = playerUid,
            errorMessage = errorMessage
        };
        
        string jsonData = JsonUtility.ToJson(resultData);
        Debug.Log($"Unity: Sending KickPlayerResult event with data: {jsonData}");
        
        #if UNITY_WEBGL && !UNITY_EDITOR
        JsKickPlayerResult(jsonData);
        #else
        Debug.Log("Not running in WebGL build, JavaScript function not called.");
        #endif
    }

    [DllImport("__Internal")]
    private static extern void JsPlacePortalPrefab(string data);

    public static void PlacePortalPrefab(PortalPrefabPlacementData data)
    {
        string jsonData = JsonUtility.ToJson(data);
        Debug.Log($"Unity: Sending PlacePortalPrefab event with data: {jsonData}");
        
        #if UNITY_WEBGL && !UNITY_EDITOR
        JsPlacePortalPrefab(jsonData);
        #else
        Debug.Log("[PlacePortalPrefab] Would send to React: " + jsonData);
        #endif
    }

    [DllImport("__Internal")]
    private static extern void JsPortalClicked(string data);

    public static void PortalClicked(PortalClickData data)
    {
        string jsonData = JsonUtility.ToJson(data);
        Debug.Log($"Unity: Sending PortalClicked event with data: {jsonData}");
        
        #if UNITY_WEBGL && !UNITY_EDITOR
        JsPortalClicked(jsonData);
        #else
        Debug.Log("[PortalClicked] Would send to React: " + jsonData);
        #endif
    }

    [DllImport("__Internal")]
    private static extern void JsSetPortalImage(string data);

    public static void SetPortalImage(PortalData data)
    {
        string jsonData = JsonUtility.ToJson(data);
        Debug.Log($"Unity: Sending SetPortalImage event with data: {jsonData}");
        
        #if UNITY_WEBGL && !UNITY_EDITOR
        JsSetPortalImage(jsonData);
        #else
        Debug.Log("[SetPortalImage] Would send to React: " + jsonData);
        #endif
    }

    [DllImport("__Internal")]
    private static extern void JsRegisterSeatingHotspot(string hotspotDataJson);

    public static void RegisterSeatingHotspot(SeatingHotspotRegistrationData data)
    {
        string jsonData = JsonUtility.ToJson(data);
        Debug.Log($"Unity: Sending RegisterSeatingHotspot event with data: {jsonData}");
        
        #if UNITY_WEBGL && !UNITY_EDITOR
        JsRegisterSeatingHotspot(jsonData);
        #else
        Debug.Log("[RegisterSeatingHotspot] Would send to React: " + jsonData);
        #endif
    }

    [DllImport("__Internal")]
    private static extern void JsSeatingHotspotClicked(string hotspotClickDataJson);

    public static void SeatingHotspotClicked(SeatingHotspotClickData data)
    {
        string jsonData = JsonUtility.ToJson(data);
        Debug.Log($"Unity: Sending SeatingHotspotClicked event with data: {jsonData}");
        
        #if UNITY_WEBGL && !UNITY_EDITOR
        JsSeatingHotspotClicked(jsonData);
        #else
        Debug.Log("[SeatingHotspotClicked] Would send to React: " + jsonData);
        #endif
    }

    [DllImport("__Internal")]
    private static extern void JsUpdateSeatingHotspotTransform(string jsonData);

    public static void UpdateSeatingHotspotTransform(SeatingHotspotTransformData data)
    {
        string jsonData = JsonUtility.ToJson(data);
        Debug.Log($"Unity: Sending UpdateSeatingHotspotTransform event with data: {jsonData}");
        #if UNITY_WEBGL && !UNITY_EDITOR
        JsUpdateSeatingHotspotTransform(jsonData);
        #else
        Debug.Log("[UpdateSeatingHotspotTransform] Would send to React: " + jsonData);
        #endif
    }
}

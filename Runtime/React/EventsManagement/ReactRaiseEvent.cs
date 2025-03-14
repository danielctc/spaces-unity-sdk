using UnityEngine;
using System.Runtime.InteropServices;
using Spaces.React.Runtime; 

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
}

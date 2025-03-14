/*
These are Javascript functions for dispatching events to React.

Called from methods in:
/Scripts/React/Events/RaiseEvents.cs
*/

mergeInto(LibraryManager.library, {
  JsFirstSceneLoaded: function () {
    console.log("Unity: Dispatching FirstSceneLoaded event");
    window.dispatchReactUnityEvent("FirstSceneLoaded");
  },

  JsRequestUserForUnity: function () {
    console.log("Unity: Dispatching RequestUserForUnity event");
    window.dispatchReactUnityEvent("RequestUserForUnity");
  },

  JsStoreUserData: function (objectPointer) {
    var jsonData = UTF8ToString(objectPointer);
    console.log("Unity: Dispatching StoreUserData event with data:", jsonData);
    window.dispatchReactUnityEvent("StoreUserData", jsonData);
  },

  JsPlayVideo: function (objectPointer) {
    var jsonData = UTF8ToString(objectPointer);
    console.log("Unity: Dispatching PlayVideo event with data:", jsonData);
    window.dispatchReactUnityEvent("PlayVideo", jsonData);
  },

  // Tests
  JsHelloFromUnity: function (objectPointer) {
    var jsonData = UTF8ToString(objectPointer);
    console.log("Unity: Dispatching HelloFromUnity event with data:", jsonData);
    window.dispatchReactUnityEvent("HelloFromUnity", jsonData);
  },

  // New function for player instantiation
  JsPlayerInstantiated: function () {
    console.log("Unity: Dispatching PlayerInstantiated event");

    // Wrap the event in a CustomEvent with an empty detail
    var event = new CustomEvent("PlayerInstantiated", { detail: {} });
    window.dispatchEvent(event);
  },

  // New function for nameplate click
  JsOpenNameplateModal: function (objectPointer) {
    var jsonData = UTF8ToString(objectPointer);
    console.log("Unity: Dispatching OpenNameplateModal event with data:", jsonData);
    window.dispatchReactUnityEvent("OpenNameplateModal", jsonData);
  },

  // New function for updating player list
  JsUpdatePlayerList: function (objectPointer) {
    var jsonData = UTF8ToString(objectPointer);
    console.log("Unity: Dispatching UpdatePlayerList event with data:", jsonData);
    window.dispatchReactUnityEvent("UpdatePlayerList", jsonData);
  },

  // New function for registering a MediaScreen
  JsRegisterMediaScreen: function (objectPointer) {
    var jsonData = UTF8ToString(objectPointer);
    console.log("Unity: Dispatching RegisterMediaScreen event with data:", jsonData);
    window.dispatchReactUnityEvent("RegisterMediaScreen", jsonData);
  },
  
  // New function for sending MediaScreen clicks
  JsSendMediaScreenClick: function (objectPointer) {
    var jsonData = UTF8ToString(objectPointer);
    console.log("Unity: Dispatching MediaScreenClick event with data:", jsonData);
    window.dispatchReactUnityEvent("MediaScreenClick", jsonData);
  },
  
  // New function for setting images on MediaScreens
  JsSetMediaScreenImage: function (objectPointer) {
    var jsonData = UTF8ToString(objectPointer);
    console.log("Unity: Dispatching SetMediaScreenImage event with data:", jsonData);
    window.dispatchReactUnityEvent("SetMediaScreenImage", jsonData);
  },
  
  // New function for setting thumbnails on MediaScreens
  JsSetMediaScreenThumbnail: function (objectPointer) {
    var jsonData = UTF8ToString(objectPointer);
    console.log("Unity: Dispatching SetMediaScreenThumbnail event with data:", jsonData);
    window.dispatchReactUnityEvent("SetMediaScreenThumbnail", jsonData);
  },
  
  // New function for forcing updates on MediaScreens
  JsForceUpdateMediaScreen: function (objectPointer) {
    var jsonData = UTF8ToString(objectPointer);
    console.log("Unity: Dispatching ForceUpdateMediaScreen event with data:", jsonData);
    window.dispatchReactUnityEvent("ForceUpdateMediaScreen", jsonData);
  },
  
  // New function for playing videos on MediaScreens
  JsPlayMediaScreenVideo: function (objectPointer) {
    var jsonData = UTF8ToString(objectPointer);
    console.log("Unity: Dispatching PlayMediaScreenVideo event with data:", jsonData);
    window.dispatchReactUnityEvent("PlayMediaScreenVideo", jsonData);
  }
});

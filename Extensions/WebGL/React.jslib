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

  // New function for toggling edit mode
  JsToggleEditMode: function (isEditMode) {
    console.log("Unity: Dispatching ToggleEditMode event with data:", isEditMode);
    window.dispatchReactUnityEvent("ToggleEditMode", isEditMode ? "true" : "false");
  },

  // New function for opening the edit modal
  JsOpenEditModal: function (objectPointer) {
    var jsonData = UTF8ToString(objectPointer);
    console.log("Unity: Dispatching OpenEditModal event with data:", jsonData);
    window.dispatchReactUnityEvent("OpenEditModal", jsonData);
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
  }
});

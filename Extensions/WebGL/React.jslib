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

  // New function for registering a Portal
  JsRegisterPortal: function (objectPointer) {
    var jsonData = UTF8ToString(objectPointer);
    console.log("Unity: Dispatching RegisterPortal event with data:", jsonData);
    window.dispatchReactUnityEvent("RegisterPortal", jsonData);
  },

  // New function for sending Portal clicks
  JsSendPortalClick: function (objectPointer) {
    var jsonData = UTF8ToString(objectPointer);
    console.log("Unity: Dispatching PortalClick event with data:", jsonData);
    window.dispatchReactUnityEvent("PortalClick", jsonData);
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
  },
  
  // New function for notifying Unity about keyboard capture state using SendMessage
  JsKeyboardCaptureRequest: function (objectPointer) {
    var jsonData = UTF8ToString(objectPointer);
    console.log("Unity: Processing KeyboardCaptureRequest with data:", jsonData);
    
    try {
      var data = JSON.parse(jsonData);
      
      // Use SendMessage to directly set the keyboard capture state
      // This is more reliable than the event system for input handling
      if (typeof unityInstance !== 'undefined') {
        console.log("Unity: Sending SetCaptureAllKeyboardInput via SendMessage with value:", data.captureKeyboard);
        unityInstance.SendMessage("WebGLInput", "SetCaptureAllKeyboardInput", data.captureKeyboard);
        
        // Add compatibility with the React approach of directly modifying WebGLInputHandler
        if (unityInstance.Module) {
          if (data.captureKeyboard === false) {
            // If disabling capture, store the handler if needed and clear it
            if (typeof window._originalWebGLInputHandler === 'undefined' && 
                unityInstance.Module.WebGLInputHandler) {
              window._originalWebGLInputHandler = unityInstance.Module.WebGLInputHandler;
            }
            // Set handler to null to stop keyboard capture
            unityInstance.Module.WebGLInputHandler = null;
            console.log("Unity: Disabled WebGLInputHandler for React compatibility");
          } else {
            // If enabling capture, restore the original handler
            if (typeof window._originalWebGLInputHandler !== 'undefined') {
              unityInstance.Module.WebGLInputHandler = window._originalWebGLInputHandler;
              console.log("Unity: Restored WebGLInputHandler for React compatibility");
            }
          }
        }
      } else {
        console.warn("Unity: unityInstance not available, cannot send SendMessage");
        
        // Fall back to event system
        window.dispatchReactUnityEvent("KeyboardCaptureRequest", jsonData);
      }
    } catch (error) {
      console.error("Unity: Error processing KeyboardCaptureRequest:", error);
      
      // Fall back to event system
      window.dispatchReactUnityEvent("KeyboardCaptureRequest", jsonData);
    }
  },
  
  // Add RunJavaScript function to run JS from Unity
  RunJavaScript: function(jsCodePtr) {
    var jsCode = UTF8ToString(jsCodePtr);
    try {
      console.log('[Unity] Executing JavaScript');
      eval(jsCode);
    } catch(e) {
      console.error('[Unity] Error executing JavaScript:', e);
    }
  },

  // New function for setting HLS Stream URL
  JsSetHLSStream: function (objectPointer) {
    var jsonData = UTF8ToString(objectPointer);
    console.log("Unity: Dispatching SetHLSStream event with data:", jsonData);
    window.dispatchReactUnityEvent("SetHLSStream", jsonData);
  },

  JsPlacePrefab: function(objectPointer) {
    var jsonData = UTF8ToString(objectPointer);
    console.log("Unity: Dispatching PlacePrefab event with data:", jsonData);
    window.dispatchReactUnityEvent("PlacePrefab", jsonData);
  },

  // New function for kick player requests and results
  JsKickPlayerResult: function (objectPointer) {
    var jsonData = UTF8ToString(objectPointer);
    console.log("Unity: Dispatching KickPlayerResult event with data:", jsonData);
    window.dispatchReactUnityEvent("KickPlayerResult", jsonData);
  },

  // Add this function to ensure we have a complete implementation
  JsKickPlayer: function (objectPointer) {
    var jsonData = UTF8ToString(objectPointer);
    console.log("Unity: Dispatching KickPlayer event with data:", jsonData);
    window.dispatchReactUnityEvent("KickPlayer", jsonData);
  },

  // New function for Portal Clicked event
  JsPortalClicked: function(objectPointer) {
    var jsonData = UTF8ToString(objectPointer);
    console.log("Unity: Dispatching PortalClicked event with data:", jsonData);
    window.dispatchReactUnityEvent("PortalClicked", jsonData);
  },

  // New function for updating portal transforms
  JsUpdatePortalTransform: function(objectPointer) {
    var jsonData = UTF8ToString(objectPointer);
    console.log("Unity: Dispatching UpdatePortalTransform event with data:", jsonData);
    window.dispatchReactUnityEvent("UpdatePortalTransform", jsonData);
  },

  // New function for placing Portal prefabs
  JsPlacePortalPrefab: function(objectPointer) {
    var jsonData = UTF8ToString(objectPointer);
    console.log("Unity: Dispatching PlacePortalPrefab event with data:", jsonData);
    window.dispatchReactUnityEvent("PlacePortalPrefab", jsonData);
  },

  // New function for setting portal images
  JsSetPortalImage: function(objectPointer) {
    var jsonData = UTF8ToString(objectPointer);
    console.log("Unity: Dispatching SetPortalImage event with data:", jsonData);
    
    // Send directly to Unity using the event system
    window.dispatchReactUnityEvent("SetPortalImage", jsonData);
  },

  // New function for registering seating hotspots
  JsRegisterSeatingHotspot: function(objectPointer) {
    var jsonData = UTF8ToString(objectPointer);
    console.log("Unity: Dispatching RegisterSeatingHotspot event with data:", jsonData);
    window.dispatchReactUnityEvent("RegisterSeatingHotspot", jsonData);
  },

  // New function for seating hotspot clicks
  JsSeatingHotspotClicked: function(objectPointer) {
    var jsonData = UTF8ToString(objectPointer);
    console.log("Unity: Dispatching SeatingHotspotClicked event with data:", jsonData);
    window.dispatchReactUnityEvent("SeatingHotspotClicked", jsonData);
  },

  // New function for updating seating hotspot transforms from Unity
  JsUpdateSeatingHotspotTransform: function(objectPointer) {
    var jsonData = UTF8ToString(objectPointer);
    console.log("Unity: Dispatching UpdateSeatingHotspotTransform event with data:", jsonData);
    window.dispatchReactUnityEvent("UpdateSeatingHotspotTransform", jsonData);
  }
});

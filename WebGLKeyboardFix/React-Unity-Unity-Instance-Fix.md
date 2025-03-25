# Fixing the Unity Instance Timing Issue

The issue stems from a timing problem where Unity's WebGLInputController initializes before React has time to expose the `unityInstance` to the global window. Here's how to fix it:

## Solution 1: Update the React Component

```jsx
import React, { useEffect } from 'react';
import { Unity, useUnityContext } from 'react-unity-webgl';

function UnityApp() {
  // Add the onLoaded callback to set unity instance immediately
  const { unityProvider, isLoaded, unityInstance } = useUnityContext({
    loaderUrl: "Build/WebGL.loader.js",
    dataUrl: "Build/WebGL.data",
    frameworkUrl: "Build/WebGL.framework.js",
    codeUrl: "Build/WebGL.wasm",
    // Critical: Set unityInstance as soon as Unity loads
    onLoaded: (unityInstance) => {
      console.log("Unity loaded callback triggered - setting window.unityInstance immediately");
      window.unityInstance = unityInstance;
      
      // Force Unity to reinitialize its input functions now that the instance is available
      setTimeout(() => {
        if (window.unityInstance) {
          window.unityInstance.SendMessage("WebGLInput", "SetupJavaScriptFunctions");
          console.log("Explicitly told Unity to reinitialize input functions");
        }
      }, 500);
    }
  });

  // Additional safety to ensure unityInstance is set
  useEffect(() => {
    if (unityInstance) {
      console.log("Setting window.unityInstance from useEffect");
      window.unityInstance = unityInstance;
      
      // After a delay, force Unity to re-check for unityInstance
      setTimeout(() => {
        if (window.unityInstance) {
          console.log("Signaling Unity to reinitialize with available unityInstance");
          window.unityInstance.SendMessage("WebGLInput", "SetupJavaScriptFunctions");
        }
      }, 1000);
    }
    
    // Cleanup when component unmounts
    return () => {
      delete window.unityInstance;
    };
  }, [unityInstance]);

  return (
    <Unity 
      unityProvider={unityProvider} 
      tabIndex={1}
      style={{ width: "800px", height: "600px" }} 
    />
  );
}

export default UnityApp;
```

## Solution 2: Add a Global Callback in index.html

Add this script directly to your HTML file, before the React component loads:

```html
<!-- Add to index.html before the React app loads -->
<script>
  // Global callback that React can trigger when Unity is ready
  window.unityReadyCallback = function(unityInstance) {
    console.log("Unity instance received via callback");
    window.unityInstance = unityInstance;
    
    // Try to notify the WebGLInput component
    setTimeout(function() {
      if (window.unityInstance) {
        try {
          window.unityInstance.SendMessage("WebGLInput", "SetupJavaScriptFunctions");
          console.log("Notified Unity to reinitialize input functions");
        } catch (e) {
          console.error("Failed to notify Unity:", e);
        }
      }
    }, 1000);
  };
</script>
```

Then update your React component to call this callback:

```jsx
useEffect(() => {
  if (unityInstance) {
    window.unityInstance = unityInstance;
    
    // Call the global callback if it exists
    if (typeof window.unityReadyCallback === 'function') {
      window.unityReadyCallback(unityInstance);
    }
  }
}, [unityInstance]);
```

## Solution 3: Add Direct Manual Helper Functions

If Unity still can't see the unityInstance, define the helper functions directly in React:

```jsx
useEffect(() => {
  if (unityInstance) {
    // Set unityInstance globally
    window.unityInstance = unityInstance;
    
    // Define helper functions directly
    window.unityCaptureKeyboard = function(capture) {
      console.log("[React] Manual unityCaptureKeyboard called with:", capture);
      return new Promise((resolve, reject) => {
        try {
          if (window.unityInstance) {
            window.unityInstance.SendMessage('WebGLInput', 'SetCaptureAllKeyboardInput', capture);
            console.log("[React] Successfully sent keyboard capture command:", capture);
            resolve(true);
          } else {
            console.warn("[React] unityInstance not found");
            reject(new Error('unityInstance not found'));
          }
        } catch (err) {
          console.error("[React] Error in unityCaptureKeyboard:", err);
          reject(err);
        }
      });
    };
    
    window.unityFocus = function() {
      console.log("[React] Manual unityFocus called");
      return new Promise((resolve, reject) => {
        try {
          if (window.unityInstance) {
            window.unityInstance.SendMessage('WebGLInput', 'FocusUnity');
            console.log("[React] Successfully sent focus command");
            resolve(true);
          } else {
            console.warn("[React] unityInstance not found");
            reject(new Error('unityInstance not found'));
          }
        } catch (err) {
          console.error("[React] Error in unityFocus:", err);
          reject(err);
        }
      });
    };
    
    console.log("Manually defined Unity helper functions");
  }
}, [unityInstance]);
```

## Testing the Connection

Add these debug buttons to verify the communication is working:

```jsx
<button onClick={() => {
  if (window.unityInstance) {
    window.unityInstance.SendMessage("WebGLInput", "TestBrowserCommunication");
    console.log("Test message sent to Unity");
  } else {
    console.error("unityInstance not available for testing");
  }
}}>
  Test Unity Communication
</button>

<button onClick={() => {
  if (window.unityInstance) {
    window.unityInstance.SendMessage("WebGLInput", "SetupJavaScriptFunctions");
    console.log("Explicitly requested Unity to reinitialize JS functions");
  } else {
    console.error("unityInstance not available for reinitialization");
  }
}}>
  Reinitialize Unity Functions
</button>
```

## Key Points to Remember

1. The WebGLInputController initializes during Unity's startup, often before React has set `window.unityInstance`
2. You need to ensure `window.unityInstance` is set as early as possible in the load process
3. After setting `window.unityInstance`, explicitly tell Unity to reinitialize its functions
4. The onLoaded callback in useUnityContext is the earliest point to set the instance
5. Add multiple fallback mechanisms to ensure the connection is established 
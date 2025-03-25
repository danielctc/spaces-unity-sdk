# Unity WebGL and React Keyboard Integration - Complete Guide

This document outlines how to properly integrate Unity WebGL keyboard input with React, including comprehensive troubleshooting steps.

## Common Issues & Solutions

### 1. "Uncaught TypeError: window.unityCaptureKeyboard(...).catch is not a function"

This error occurs when React tries to use `.catch()` on a function that doesn't return a Promise.

**Solution:** The `WebGLInputController.cs` now uses Promise-based JavaScript functions:

```javascript
// In SetupJavaScriptFunctions():
window.unityCaptureKeyboard = function(capture) {
  return new Promise((resolve, reject) => {
    try {
      if (window.unityInstance) {
        window.unityInstance.SendMessage('WebGLInput', 'SetCaptureAllKeyboardInput', capture);
        resolve(true);
      } else {
        reject(new Error('unityInstance not found'));
      }
    } catch (err) {
      reject(err);
    }
  });
};
```

### 2. Missing unityInstance: "[Unity] unityInstance not found"

This occurs when React tries to use Unity functions before Unity has fully loaded.

**Solution:**

In your React component:

```jsx
// Define the helper functions manually in React
useEffect(() => {
  if (unityInstance) {
    // Set the global instance
    window.unityInstance = unityInstance;
    
    // Log to verify it's set correctly
    console.log("Unity instance exposed to window", window.unityInstance);
    
    // Force setup of JavaScript functions by calling the method directly
    window.unityInstance.SendMessage("WebGLInput", "SetupJavaScriptFunctions");
  }
}, [unityInstance]);
```

### 3. Verifying Unity <-> React Communication

Use these browser console commands to verify proper communication:

```javascript
// Check Unity instance
console.log("Unity instance exists:", !!window.unityInstance);

// Check helper functions
console.log("unityCaptureKeyboard exists:", typeof window.unityCaptureKeyboard === 'function');
console.log("unityFocus exists:", typeof window.unityFocus === 'function');

// Check canvas
const canvas = document.querySelector('canvas');
console.log("Canvas found:", !!canvas);
console.log("Canvas tabIndex:", canvas ? canvas.getAttribute('tabindex') : 'none');

// Test using unityCaptureKeyboard
if (window.unityCaptureKeyboard) {
  window.unityCaptureKeyboard(true)
    .then(() => console.log("Successfully enabled keyboard capture"))
    .catch(e => console.error("Failed to enable keyboard capture:", e));
}
```

## Complete Integration Setup

### 1. Unity Side

1. **Create WebGLInput GameObject:**
   - Create an empty GameObject named exactly "WebGLInput"
   - Attach `WebGLInputController.cs` to it
   - Set `debugLogging` to true during development

2. **Verify Unity Setup:**
   - Ensure React.jslib contains the RunJavaScript function
   - Check for compiler errors
   - Ensure WebGLInput.captureAllKeyboardInput is accessible
   - Build WebGL with Development Build enabled

### 2. React Side

1. **Expose unityInstance:**

```jsx
import React, { useEffect, useState } from 'react';
import { Unity, useUnityContext } from 'react-unity-webgl';

function UnityComponent() {
  const { unityProvider, isLoaded, unityInstance } = useUnityContext({
    loaderUrl: "Build/myGame.loader.js",
    dataUrl: "Build/myGame.data",
    frameworkUrl: "Build/myGame.framework.js",
    codeUrl: "Build/myGame.wasm",
  });
  
  const [unityReady, setUnityReady] = useState(false);

  // CRITICAL: Expose unityInstance AND initialize helper functions
  useEffect(() => {
    if (unityInstance) {
      // Step 1: Expose unityInstance globally
      window.unityInstance = unityInstance;
      console.log("Exposed unityInstance to window:", !!window.unityInstance);
      
      // Step 2: If helper functions aren't initialized by Unity, define them here
      if (typeof window.unityCaptureKeyboard !== 'function') {
        window.unityCaptureKeyboard = function(capture) {
          return new Promise((resolve, reject) => {
            try {
              if (window.unityInstance) {
                window.unityInstance.SendMessage('WebGLInput', 'SetCaptureAllKeyboardInput', capture);
                resolve(true);
              } else {
                reject(new Error('unityInstance not found'));
              }
            } catch (err) {
              reject(err);
            }
          });
        };
        
        window.unityFocus = function() {
          return new Promise((resolve, reject) => {
            try {
              if (window.unityInstance) {
                window.unityInstance.SendMessage('WebGLInput', 'FocusUnity');
                resolve(true);
              } else {
                reject(new Error('unityInstance not found'));
              }
            } catch (err) {
              reject(err);
            }
          });
        };
        
        console.log("Manually defined Unity helper functions");
      }
      
      // Step 3: Wait a moment then check if everything is set up
      setTimeout(() => {
        const unityCaptureKeyboardExists = typeof window.unityCaptureKeyboard === 'function';
        const unityFocusExists = typeof window.unityFocus === 'function';
        
        console.log("Unity integration status:", {
          unityInstance: !!window.unityInstance,
          unityCaptureKeyboard: unityCaptureKeyboardExists,
          unityFocus: unityFocusExists
        });
        
        setUnityReady(unityCaptureKeyboardExists && unityFocusExists);
      }, 1000);
    }
    
    return () => {
      delete window.unityInstance;
    };
  }, [unityInstance]);

  return (
    <div>
      <Unity 
        unityProvider={unityProvider} 
        tabIndex={1}
        style={{ width: "800px", height: "600px" }} 
      />
      <div>
        <p>Unity Ready: {unityReady ? "YES" : "NO"}</p>
        {unityReady && (
          <button onClick={() => window.unityFocus()}>Focus Unity</button>
        )}
      </div>
    </div>
  );
}
```

2. **Safe Input Handling:**

```jsx
function ReactInput() {
  const handleInputFocus = () => {
    // Check if Unity is ready before trying to use it
    if (window.unityCaptureKeyboard) {
      window.unityCaptureKeyboard(false)
        .catch(err => console.warn("Could not disable Unity keyboard:", err));
    } else if (window.unityInstance) {
      // Direct approach if helper functions aren't available
      window.unityInstance.SendMessage("WebGLInput", "DisableKeyboardCapture");
    } else {
      console.warn("Unity is not ready, can't disable keyboard capture");
    }
  };

  return (
    <input 
      type="text"
      onFocus={handleInputFocus}
      placeholder="Type here..."
    />
  );
}
```

## Debugging Tips

1. **Check Browser Console:**
   - Look for errors related to unityInstance
   - Check for "[Unity]" prefixed log messages

2. **Add Debug Hooks:**
   - Add a debug button to force re-initialization:
     ```jsx
     <button onClick={() => {
       if (window.unityInstance) {
         window.unityInstance.SendMessage("WebGLInput", "SetupJavaScriptFunctions");
         console.log("Manually reinitializing Unity input functions");
       }
     }}>Reinitialize Unity Input</button>
     ```

3. **Use ReactRaiseEvent:**
   - If direct SendMessage isn't working, try using ReactRaiseEvent's KeyboardCaptureRequest:
     ```csharp
     // In Unity C#
     ReactRaiseEvent.SendKeyboardCaptureRequest(new KeyboardCaptureRequestData { captureKeyboard = false });
     ```

4. **Timing Issues:**
   - The most common problems are timing-related
   - Always check if unityInstance exists before using it
   - Use Promise.catch() to handle errors gracefully
   - Consider adding a wait period between Unity loading and keyboard control

## Additional Resources

1. [React Unity WebGL tabIndex Docs](https://react-unity-webgl.dev/docs/api/tab-index)
2. [Unity WebGL Input System](https://docs.unity3d.com/Manual/webgl-input.html)
3. [Promise-based API pattern](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Guide/Using_promises) 
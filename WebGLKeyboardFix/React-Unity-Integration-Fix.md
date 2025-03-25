# Unity-React Integration Fix for SpaceManageModal

This guide explains how to fix the keyboard capture issue between Unity WebGL and React components like SpaceManageModal.

## Current Issue

Based on the error logs, your SpaceManageModal is trying to call `window.unityCaptureKeyboard(false)` but is getting:

```
SpaceManageModal.jsx:112 SpaceManageModal: Error disabling Unity keyboard: Error: unityInstance not found
```

This happens because the React component is trying to interact with Unity before `unityInstance` has been properly initialized and made available globally.

## Solution

### 1. In your React component where Unity is initialized:

Add the following code to register the Unity instance as soon as it's available:

```jsx
// In the component that initializes Unity (likely using useUnityContext)
import { Unity, useUnityContext } from 'react-unity-webgl';

function UnityComponent() {
  const { unityProvider, isLoaded, unityInstance } = useUnityContext({
    loaderUrl: "Build/your-game.loader.js", 
    dataUrl: "Build/your-game.data",
    frameworkUrl: "Build/your-game.framework.js",
    codeUrl: "Build/your-game.wasm",
  });

  // Register unityInstance as soon as it's available
  useEffect(() => {
    if (unityInstance) {
      // Set directly on window
      window.unityInstance = unityInstance;
      console.log("Unity instance set directly:", !!window.unityInstance);
      
      // Also use the registerUnityInstance function if it exists
      if (typeof window.registerUnityInstance === 'function') {
        window.registerUnityInstance(unityInstance);
        console.log("Unity instance registered with registerUnityInstance");
      }
    }
    
    return () => {
      delete window.unityInstance;
    };
  }, [unityInstance]);

  return (
    <Unity 
      unityProvider={unityProvider} 
      tabIndex={1}
      style={{ width: '100%', height: '100%' }} 
    />
  );
}
```

### 2. Update your SpaceManageModal component to handle errors gracefully:

```jsx
// In SpaceManageModal.jsx

// Function to safely disable Unity keyboard capture
const safeDisableKeyboard = async () => {
  try {
    console.log("SpaceManageModal: Trying to disable Unity keyboard");
    
    // First check if unityInstance is available directly
    if (window.unityInstance) {
      window.unityInstance.SendMessage("WebGLInput", "SetCaptureAllKeyboardInput", false);
      console.log("SpaceManageModal: Disabled Unity keyboard via direct SendMessage");
      return true;
    }
    
    // If not, try the helper function
    if (typeof window.unityCaptureKeyboard === 'function') {
      await window.unityCaptureKeyboard(false);
      console.log("SpaceManageModal: Disabled Unity keyboard via helper function");
      return true;
    }
    
    // Last resort - use sync version
    if (typeof window.unityCaptureKeyboardSync === 'function') {
      const result = window.unityCaptureKeyboardSync(false);
      console.log("SpaceManageModal: Attempted to disable Unity keyboard via sync function, result:", result);
      // Even if this returns false, it will have stored the request for when Unity is ready
      return true;
    }
    
    console.warn("SpaceManageModal: No Unity keyboard control methods available");
    return false;
  } catch (error) {
    console.log("SpaceManageModal: Error disabling Unity keyboard:", error);
    // Don't rethrow - we want to continue opening the modal even if keyboard control fails
    return false;
  }
};

// Use in useEffect or open handler
useEffect(() => {
  if (modalOpen) {
    safeDisableKeyboard();
  } else {
    // Similar safe enable function for when modal closes
  }
}, [modalOpen]);
```

### 3. Additional fallback mechanism for React.js

Add this global listener to catch and handle cases when the Unity instance isn't available yet:

```jsx
// Add to your main App.jsx or similar top-level component
useEffect(() => {
  // Function to handle messages about keyboard capture
  const handleMessage = (event) => {
    // If Unity is asking React to handle keyboard input
    if (event.data?.type === 'KeyboardCaptureRequest' && 
        event.data?.data?.captureKeyboard === false) {
      console.log('React received keyboard capture request from Unity');
      // Your modal opening code here
    }
  };

  window.addEventListener('message', handleMessage);
  
  return () => {
    window.removeEventListener('message', handleMessage);
  };
}, []);
```

## How It Works

1. The Unity `WebGLInputController` now defines a `registerUnityInstance` function that React can use
2. When Unity can't capture keyboard input because `unityInstance` isn't available, it stores the request
3. Once React provides the `unityInstance`, Unity will apply the pending keyboard capture settings
4. The React component handles errors gracefully and doesn't break when Unity isn't ready

This solution ensures that keyboard focus works correctly regardless of the initialization order of Unity and React components. 
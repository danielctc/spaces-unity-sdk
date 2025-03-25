# Unity WebGL Keyboard Input for React

This package provides a simple solution for handling keyboard input sharing between Unity WebGL and React components.

## The Problem

By default, Unity WebGL builds capture all keyboard input, which prevents React components like inputs, textareas, and modals from receiving keyboard events.

## Setup Instructions

### Unity Setup

1. Create an empty GameObject named "WebGLInput" in your scene
2. Attach the `WebGLInputController.cs` script to it
3. Configure settings as needed:
   - Enable `startWithKeyboardCaptureDisabled` if you want React to have keyboard focus initially
   - Enable `debugLogging` for troubleshooting

### React Setup

1. Make sure you're using [react-unity-webgl](https://github.com/jeffreylanters/react-unity-webgl)
2. Set `tabIndex` on your Unity component:

```jsx
import { Unity, useUnityContext } from "react-unity-webgl";

function App() {
  const { unityProvider, isLoaded, unityInstance } = useUnityContext({
    loaderUrl: "build/myGame.loader.js",
    dataUrl: "build/myGame.data",
    frameworkUrl: "build/myGame.framework.js",
    codeUrl: "build/myGame.wasm",
  });

  // Important: Expose unityInstance to window
  React.useEffect(() => {
    if (unityInstance) {
      window.unityInstance = unityInstance;
    }
    return () => {
      delete window.unityInstance;
    };
  }, [unityInstance]);

  return (
    <Unity 
      unityProvider={unityProvider} 
      tabIndex={1} // This makes the canvas focusable
      style={{ width: "100%", height: "100%" }} 
    />
  );
}
```

## Usage in React

### Method 1: Direct SendMessage (Recommended)

The most reliable approach is to use SendMessage directly:

```jsx
function MyComponent() {
  // Disable Unity keyboard capture when focusing an input
  const handleInputFocus = () => {
    if (window.unityInstance) {
      window.unityInstance.SendMessage("WebGLInput", "DisableKeyboardCapture");
    }
  };

  // Re-enable Unity keyboard capture when blurring an input
  const handleInputBlur = () => {
    if (window.unityInstance) {
      window.unityInstance.SendMessage("WebGLInput", "EnableKeyboardCapture");
    }
  };

  // Focus Unity canvas
  const focusUnity = () => {
    if (window.unityInstance) {
      window.unityInstance.SendMessage("WebGLInput", "FocusUnity");
    }
  };

  return (
    <div>
      <input 
        type="text" 
        onFocus={handleInputFocus} 
        onBlur={handleInputBlur} 
        placeholder="Type here"
      />
      <button onClick={focusUnity}>Focus Unity</button>
    </div>
  );
}
```

### Method 2: Promise-based API

For frameworks that expect Promise returns, you can use the global helper functions:

```jsx
function MyComponent() {
  const [error, setError] = useState(null);

  // Disable Unity keyboard capture with Promise API
  const handleInputFocus = () => {
    window.unityCaptureKeyboard(false)
      .catch(err => {
        console.error("Failed to disable Unity keyboard:", err);
        setError("Keyboard control error");
      });
  };

  // Re-enable Unity keyboard capture with Promise API
  const handleInputBlur = () => {
    window.unityCaptureKeyboard(true)
      .catch(err => {
        console.error("Failed to enable Unity keyboard:", err);
      });
  };

  // Focus Unity with Promise API
  const focusUnity = () => {
    window.unityFocus()
      .then(() => console.log("Unity focused successfully"))
      .catch(err => console.error("Failed to focus Unity:", err));
  };

  return (
    <div>
      {error && <div className="error">{error}</div>}
      <input 
        type="text" 
        onFocus={handleInputFocus} 
        onBlur={handleInputBlur} 
        placeholder="Type here"
      />
      <button onClick={focusUnity}>Focus Unity</button>
    </div>
  );
}
```

## How It Works

The `WebGLInputController` script:

1. Ensures the Unity canvas has `tabIndex` set to make it focusable
2. Exposes public methods that can be called via `SendMessage` from JavaScript
3. Provides global helper functions in the browser that return Promises for frameworks that need them:
   - `window.unityCaptureKeyboard(boolean)` - Controls keyboard capture
   - `window.unityFocus()` - Focuses the Unity canvas

## Troubleshooting

- If keyboard events aren't working, check the browser console for errors
- Make sure you've set `tabIndex` on your Unity component in React
- Ensure you've exposed `unityInstance` to the global window
- Try calling `window.unityFocus()` from the browser console to test functionality
- If your framework uses Promises, make sure you're handling rejections with `.catch()` 
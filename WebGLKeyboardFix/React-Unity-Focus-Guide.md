# React Integration Guide for Unity WebGL Keyboard Focus

This guide explains how to properly manage keyboard focus between React UI and Unity WebGL content.

## Key Concepts

1. **Unity WebGL captures all keyboard input by default**
2. **Focus must be properly managed when opening/closing modals**
3. **The Unity canvas needs tabIndex to receive keyboard events**
4. **Unity instance must be accessible via window.unityInstance**

## Setup in React

### 1. Expose Unity Instance Globally

In your main Unity component:

```jsx
// In your Unity component
import { Unity, useUnityContext } from 'react-unity-webgl';

function UnityComponent() {
  const { unityProvider, isLoaded, unityInstance } = useUnityContext({
    loaderUrl: "Build/your-game.loader.js",
    dataUrl: "Build/your-game.data",
    frameworkUrl: "Build/your-game.framework.js",
    codeUrl: "Build/your-game.wasm",
  });

  // CRITICAL: Expose unityInstance globally
  useEffect(() => {
    if (unityInstance) {
      window.unityInstance = unityInstance;
      console.log("Unity instance exposed globally");
    }
    return () => {
      delete window.unityInstance;
    };
  }, [unityInstance]);

  return (
    <Unity 
      unityProvider={unityProvider} 
      tabIndex={1} // Important for keyboard focus
      style={{ width: '100%', height: '100%' }} 
    />
  );
}
```

### 2. Manage Focus When Using Modals

When opening any modal that should receive keyboard input:

```jsx
function openModal() {
  // Disable Unity keyboard capture
  if (window.unityInstance) {
    window.unityInstance.SendMessage('WebGLInput', 'SetCaptureAllKeyboardInput', false);
  } else if (window.unityCaptureKeyboard) {
    window.unityCaptureKeyboard(false);
  }
  
  // Then open your modal
  setModalOpen(true);
}
```

When closing a modal, restore Unity focus:

```jsx
function closeModal() {
  // First close your modal
  setModalOpen(false);
  
  // Wait for React to finish unmounting the modal
  setTimeout(() => {
    // Then restore Unity focus
    if (window.unityFocus) {
      window.unityFocus();
    } else if (window.unityInstance) {
      window.unityInstance.SendMessage('WebGLInput', 'FocusUnity');
    }
  }, 100); // Adjust delay if needed
}
```

### 3. Focus Management Hook

Create a reusable hook for Unity focus management:

```jsx
function useUnityFocus() {
  const disableUnityKeyboard = useCallback(() => {
    if (window.unityInstance) {
      window.unityInstance.SendMessage('WebGLInput', 'SetCaptureAllKeyboardInput', false);
      return true;
    } else if (window.unityCaptureKeyboard) {
      window.unityCaptureKeyboard(false);
      return true;
    }
    console.warn("Could not disable Unity keyboard capture");
    return false;
  }, []);

  const enableUnityKeyboard = useCallback(() => {
    if (window.unityInstance) {
      window.unityInstance.SendMessage('WebGLInput', 'SetCaptureAllKeyboardInput', true);
      return true;
    } else if (window.unityCaptureKeyboard) {
      window.unityCaptureKeyboard(true);
      return true;
    }
    console.warn("Could not enable Unity keyboard capture");
    return false;
  }, []);

  const focusUnity = useCallback(() => {
    if (window.unityFocus) {
      window.unityFocus();
      return true;
    } else if (window.unityInstance) {
      window.unityInstance.SendMessage('WebGLInput', 'FocusUnity');
      return true;
    }
    console.warn("Could not focus Unity");
    return false;
  }, []);

  return { disableUnityKeyboard, enableUnityKeyboard, focusUnity };
}
```

### 4. Input Components with Auto-Focus Management

Create wrapper components that handle Unity focus automatically:

```jsx
function UnityAwareInput({ ...props }) {
  const { disableUnityKeyboard, enableUnityKeyboard } = useUnityFocus();
  
  return (
    <input
      {...props}
      onFocus={() => disableUnityKeyboard()}
      onBlur={() => enableUnityKeyboard()}
    />
  );
}
```

### 5. Modal Component with Focus Management

```jsx
function UnityAwareModal({ isOpen, onClose, children }) {
  const { disableUnityKeyboard, focusUnity } = useUnityFocus();
  
  useEffect(() => {
    if (isOpen) {
      disableUnityKeyboard();
    }
  }, [isOpen, disableUnityKeyboard]);
  
  const handleClose = () => {
    onClose();
    
    // Wait for modal to close, then refocus Unity
    setTimeout(() => {
      focusUnity();
    }, 100);
  };
  
  if (!isOpen) return null;
  
  return (
    <div className="modal">
      <div className="modal-content">
        {children}
        <button onClick={handleClose}>Close</button>
      </div>
    </div>
  );
}
```

## MediaScreen Integration

If you're using MediaScreen components, add specific handling for their events:

```jsx
// In your MediaScreen component or modal
function MediaScreenModal({ screenId, isOpen, onClose }) {
  const { focusUnity } = useUnityFocus();
  
  const handleClose = () => {
    onClose();
    
    // Give a longer delay for MediaScreen modals
    setTimeout(() => {
      focusUnity();
      console.log(`MediaScreen modal closed for screen ${screenId}, restored Unity focus`);
    }, 200); // Consider a longer delay for MediaScreen modals
  };
  
  // Rest of your modal component
}
```

## Emergency Focus Button

Consider adding a hidden emergency focus button that users can click if focus is completely lost:

```jsx
function EmergencyFocusButton() {
  const [isVisible, setIsVisible] = useState(false);
  const { focusUnity } = useUnityFocus();
  
  // Show button on Ctrl+Shift+F
  useEffect(() => {
    const handleKeyDown = (e) => {
      if (e.ctrlKey && e.shiftKey && e.key === 'F') {
        setIsVisible(prev => !prev);
      }
    };
    
    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, []);
  
  if (!isVisible) return null;
  
  return (
    <button
      onClick={() => focusUnity()}
      style={{
        position: 'fixed',
        bottom: '10px',
        right: '10px',
        zIndex: 9999,
        background: 'red',
        color: 'white',
        padding: '10px',
        border: 'none',
        borderRadius: '5px'
      }}
    >
      Focus Unity
    </button>
  );
}
```

## Troubleshooting

If keyboard focus is still problematic:

1. **Test the Unity focus functions in browser console**:
   ```javascript
   window.unityFocus(); // Focus Unity
   window.unityCaptureKeyboard(true); // Enable keyboard capture
   window.unityCaptureKeyboard(false); // Disable keyboard capture
   ```

2. **Check for missing unityInstance**:
   ```javascript
   console.log(!!window.unityInstance); // Should be true
   ```

3. **Increase delay times** in your focus management functions. Some complex modals may need longer delays.

4. **Check browser console** for log messages with these prefixes:
   - `[WebGLInputController]`
   - `[Unity WebGLInput]`
   - `[MediaScreenModalFix]`
   - `[MediaScreenEventSystemFix]` 
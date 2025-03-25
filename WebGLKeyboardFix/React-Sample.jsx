import React, { useEffect, useState } from 'react';
import { Unity, useUnityContext } from 'react-unity-webgl';

const UnityApp = () => {
  // Initialize Unity context
  const { unityProvider, isLoaded, loadingProgression, unityInstance } = useUnityContext({
    loaderUrl: "/Build/WebGL.loader.js",
    dataUrl: "/Build/WebGL.data",
    frameworkUrl: "/Build/WebGL.framework.js",
    codeUrl: "/Build/WebGL.wasm",
  });
  
  const [error, setError] = useState(null);
  const [unityReady, setUnityReady] = useState(false);
  const [unityStatus, setUnityStatus] = useState({
    instance: false,
    captureKeyboardFn: false,
    focusFn: false,
    canvas: false
  });

  // Log Unity loading state changes
  useEffect(() => {
    console.log(`Unity loading state changed: ${isLoaded ? 'LOADED' : 'LOADING'}`);
    
    if (isLoaded) {
      console.log('Unity has finished loading, unity context:', { unityProvider, unityInstance });
    }
  }, [isLoaded]);

  // CRITICAL: Expose unityInstance to window for the WebGLInputController to access
  useEffect(() => {
    console.log("Unity instance effect triggered, instance:", unityInstance);
    
    if (unityInstance) {
      console.log("Setting window.unityInstance:", unityInstance);
      window.unityInstance = unityInstance;
      
      // Verify it was set correctly
      console.log("Verification - window.unityInstance exists:", !!window.unityInstance);
      
      // Wait a short time to ensure Unity has time to initialize JS functions
      setTimeout(() => {
        console.log("Checking Unity input functions after delay");
        
        // Check what functions exist
        const status = {
          instance: !!window.unityInstance,
          captureKeyboardFn: typeof window.unityCaptureKeyboard === 'function',
          focusFn: typeof window.unityFocus === 'function',
          canvas: !!document.querySelector('canvas')
        };
        console.log("Unity status:", status);
        setUnityStatus(status);
        
        // If functions don't exist but instance does, define them manually
        if (!status.captureKeyboardFn && status.instance) {
          console.log("Unity helper functions not found, defining manually");
          
          // Define keyboard capture function
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
          
          // Define focus function
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
          
          console.log("[React] Manually defined Unity helper functions");
          
          // Update status
          setUnityStatus(prev => ({
            ...prev,
            captureKeyboardFn: true,
            focusFn: true
          }));
        }
        
        // If we have functions or just defined them, consider Unity ready
        if (status.instance && (status.captureKeyboardFn || status.focusFn)) {
          console.log("Unity input system ready!");
          setUnityReady(true);
        } else {
          console.warn("Unity input system not fully ready");
          
          // Try calling setup function directly as a last resort
          if (status.instance) {
            console.log("Attempting to initialize Unity input functions directly");
            window.unityInstance.SendMessage('WebGLInput', 'SetupJavaScriptFunctions');
            
            // Check again after a short delay
            setTimeout(() => {
              const updatedStatus = {
                instance: !!window.unityInstance,
                captureKeyboardFn: typeof window.unityCaptureKeyboard === 'function',
                focusFn: typeof window.unityFocus === 'function'
              };
              console.log("Unity status after direct initialization:", updatedStatus);
              setUnityStatus(updatedStatus);
              setUnityReady(updatedStatus.instance && 
                          (updatedStatus.captureKeyboardFn || updatedStatus.focusFn));
            }, 1000);
          }
        }
      }, 2000); // Wait 2 seconds to ensure Unity has time to initialize
      
      // Cleanup
      return () => {
        console.log("Cleaning up unity instance");
        delete window.unityInstance;
      };
    }
  }, [unityInstance]);

  // Example input handlers to toggle Unity keyboard capture
  const handleInputFocus = () => {
    console.log("Input focused, disabling Unity keyboard capture");
    setError(null);
    
    // Direct SendMessage approach (most reliable)
    if (window.unityInstance) {
      console.log("Calling DisableKeyboardCapture via SendMessage");
      window.unityInstance.SendMessage("WebGLInput", "DisableKeyboardCapture");
      return;
    }
    
    // Alternative: Use the global helper function which returns a Promise
    if (window.unityCaptureKeyboard) {
      console.log("Calling unityCaptureKeyboard(false)");
      window.unityCaptureKeyboard(false)
        .then(() => console.log("Successfully disabled Unity keyboard capture"))
        .catch(err => {
          console.error("Error disabling Unity keyboard capture:", err);
          setError("Failed to disable Unity keyboard capture");
        });
      return;
    }
    
    console.warn("No method available to disable Unity keyboard capture");
    setError("Unity input control not available");
  };

  const handleInputBlur = () => {
    console.log("Input blurred, re-enabling Unity keyboard capture");
    setError(null);
    
    // Direct SendMessage approach (most reliable)
    if (window.unityInstance) {
      console.log("Calling EnableKeyboardCapture via SendMessage");
      window.unityInstance.SendMessage("WebGLInput", "EnableKeyboardCapture");
      return;
    }
    
    // Alternative: Use the global helper function which returns a Promise
    if (window.unityCaptureKeyboard) {
      console.log("Calling unityCaptureKeyboard(true)");
      window.unityCaptureKeyboard(true)
        .then(() => console.log("Successfully enabled Unity keyboard capture"))
        .catch(err => {
          console.error("Error enabling Unity keyboard capture:", err);
          setError("Failed to enable Unity keyboard capture");
        });
      return;
    }
    
    console.warn("No method available to enable Unity keyboard capture");
    setError("Unity input control not available");
  };

  // Focus Unity explicitly (for debug button)
  const focusUnity = () => {
    console.log("Focus Unity button clicked");
    setError(null);
    
    // Direct SendMessage approach (most reliable)
    if (window.unityInstance) {
      console.log("Calling FocusUnity via SendMessage");
      window.unityInstance.SendMessage("WebGLInput", "FocusUnity");
      return;
    }
    
    // Alternative: Use the global helper function which returns a Promise
    if (window.unityFocus) {
      console.log("Calling unityFocus()");
      window.unityFocus()
        .then(() => console.log("Successfully focused Unity"))
        .catch(err => {
          console.error("Error focusing Unity:", err);
          setError("Failed to focus Unity");
        });
      return;
    }
    
    console.warn("No method available to focus Unity");
    setError("Unity focus control not available");
  };
  
  // Function to manually reinitialize Unity input functions
  const reinitializeUnityFunctions = () => {
    console.log("Reinitializing Unity input functions");
    setError(null);
    
    if (window.unityInstance) {
      console.log("Calling SetupJavaScriptFunctions via SendMessage");
      window.unityInstance.SendMessage("WebGLInput", "SetupJavaScriptFunctions");
      
      // Check after short delay
      setTimeout(() => {
        const status = {
          instance: !!window.unityInstance,
          captureKeyboardFn: typeof window.unityCaptureKeyboard === 'function',
          focusFn: typeof window.unityFocus === 'function'
        };
        console.log("Unity status after reinitialization:", status);
        setUnityStatus(status);
        setUnityReady(status.instance && (status.captureKeyboardFn || status.focusFn));
      }, 1000);
      
      return;
    }
    
    setError("Unity instance not available for reinitialization");
  };
  
  // Test Unity communication
  const testCommunication = () => {
    console.log("Testing Unity communication");
    setError(null);
    
    if (window.unityInstance) {
      console.log("Calling TestBrowserCommunication via SendMessage");
      window.unityInstance.SendMessage("WebGLInput", "TestBrowserCommunication");
      return;
    }
    
    setError("Unity instance not available for testing");
  };

  return (
    <div className="unity-container">
      {/* Unity loading progress */}
      {!isLoaded && (
        <div className="loading-overlay" style={{
          position: 'absolute',
          top: 0,
          left: 0,
          width: '100%',
          height: '100%',
          backgroundColor: 'rgba(0,0,0,0.7)',
          color: 'white',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          zIndex: 10
        }}>
          <div>
            <h2>Loading Unity...</h2>
            <div style={{ height: '20px', width: '300px', border: '1px solid white' }}>
              <div style={{ 
                height: '100%', 
                width: `${Math.round(loadingProgression * 100)}%`,
                backgroundColor: 'white'
              }} />
            </div>
            <p>{Math.round(loadingProgression * 100)}%</p>
          </div>
        </div>
      )}
      
      {/* The Unity component with tabIndex */}
      <Unity
        unityProvider={unityProvider}
        tabIndex={1}
        className="unity-canvas"
        style={{
          width: '800px',
          height: '600px',
          background: '#555',
        }}
      />
      
      {/* Debug Status Panel */}
      <div className="unity-status" style={{
        margin: '20px 0',
        padding: '10px',
        border: '1px solid #ccc',
        borderRadius: '4px',
        backgroundColor: '#f8f8f8'
      }}>
        <h3>Unity Status</h3>
        <div style={{ display: 'flex', gap: '10px', marginBottom: '10px' }}>
          <div style={{ 
            backgroundColor: isLoaded ? '#4CAF50' : '#F44336',
            color: 'white',
            padding: '5px 10px',
            borderRadius: '4px',
            fontWeight: 'bold'
          }}>
            {isLoaded ? 'LOADED' : 'LOADING'}
          </div>
          
          <div style={{ 
            backgroundColor: unityReady ? '#4CAF50' : '#F44336',
            color: 'white',
            padding: '5px 10px',
            borderRadius: '4px',
            fontWeight: 'bold'
          }}>
            {unityReady ? 'READY' : 'NOT READY'}
          </div>
        </div>
        
        <div style={{ marginBottom: '10px' }}>
          <div>Unity Instance: {unityStatus.instance ? '✅' : '❌'}</div>
          <div>Keyboard Control: {unityStatus.captureKeyboardFn ? '✅' : '❌'}</div>
          <div>Focus Control: {unityStatus.focusFn ? '✅' : '❌'}</div>
          <div>Canvas Element: {unityStatus.canvas ? '✅' : '❌'}</div>
        </div>
        
        {error && (
          <div style={{ color: 'red', marginBottom: '10px', padding: '5px', border: '1px solid red' }}>
            Error: {error}
          </div>
        )}
        
        <div style={{ display: 'flex', gap: '10px' }}>
          <button 
            onClick={reinitializeUnityFunctions}
            style={{ padding: '5px 10px', cursor: 'pointer' }}
          >
            Reinitialize Unity Functions
          </button>
          
          <button 
            onClick={testCommunication}
            style={{ padding: '5px 10px', cursor: 'pointer' }}
          >
            Test Communication
          </button>
        </div>
      </div>
      
      {/* Example React UI that uses keyboard input */}
      <div className="react-ui" style={{ marginTop: '20px' }}>
        <h3>React UI Controls</h3>
        
        <div style={{ marginBottom: '10px' }}>
          <label>
            Text Input:
            <input
              type="text"
              onFocus={handleInputFocus}
              onBlur={handleInputBlur}
              placeholder="Type here..."
              style={{ marginLeft: '10px', padding: '5px' }}
            />
          </label>
        </div>
        
        <div style={{ marginBottom: '10px' }}>
          <label>
            Another Input:
            <input
              type="text"
              onFocus={handleInputFocus}
              onBlur={handleInputBlur}
              placeholder="More typing..."
              style={{ marginLeft: '10px', padding: '5px' }}
            />
          </label>
        </div>
        
        <button 
          onClick={focusUnity} 
          style={{ padding: '8px 16px', cursor: 'pointer', backgroundColor: '#2196F3', color: 'white', border: 'none', borderRadius: '4px' }}
        >
          Focus Unity
        </button>
      </div>
    </div>
  );
};

export default UnityApp; 
using UnityEngine;
using System.Runtime.InteropServices;

/// <summary>
/// Simple keyboard input manager for Unity WebGL + React integration
/// </summary>
public class WebGLInputController : MonoBehaviour
{
    [SerializeField, Tooltip("Enable debug logging")]
    private bool debugLogging = true;

    [SerializeField, Tooltip("If true, keyboard capture starts disabled to allow React UI to receive input")]
    private bool startWithKeyboardCaptureDisabled = false;

    [SerializeField, Tooltip("If true, enables compatibility with React's Module.WebGLInputHandler approach")]
    private bool enableReactModuleCompatibility = true;

    #if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void RunJavaScript(string jsCode);
    #endif

    private void Awake()
    {
        // Always log initialization regardless of debug setting
        Debug.Log("[WebGLInput] Initializing WebGLInputController");
        
        // Ensure the GameObject has the correct name for SendMessage to work
        if (gameObject.name != "WebGLInput")
        {
            gameObject.name = "WebGLInput";
            Debug.Log("[WebGLInput] Renamed GameObject to 'WebGLInput' for SendMessage compatibility");
        }

        DontDestroyOnLoad(gameObject);
    }
    
    private void Start()
    {
        Debug.Log("[WebGLInput] WebGLInputController Start method called");
        
        #if UNITY_WEBGL && !UNITY_EDITOR
        // Initial keyboard state
        bool initialCaptureState = !startWithKeyboardCaptureDisabled;
        WebGLInput.captureAllKeyboardInput = initialCaptureState;
        Debug.Log($"[WebGLInput] Initial WebGLInput.captureAllKeyboardInput set to {initialCaptureState}");
        
        // Set up the JavaScript functions for external control
        Debug.Log("[WebGLInput] Setting up JavaScript functions");
        InitializeJavaScriptFunctions();
        
        // Test direct communication with browser console
        TestBrowserCommunication();
        #else
        Debug.Log("[WebGLInput] Not in WebGL build, keyboard control disabled");
        #endif
    }

    /// <summary>
    /// Called from React via unityInstance.SendMessage("WebGLInput", "SetCaptureAllKeyboardInput", true/false)
    /// </summary>
    public void SetCaptureAllKeyboardInput(bool shouldCapture)
    {
        // Always log this call regardless of debug setting
        Debug.Log($"[WebGLInput] SetCaptureAllKeyboardInput called with: {shouldCapture}");

        #if UNITY_WEBGL && !UNITY_EDITOR
        WebGLInput.captureAllKeyboardInput = shouldCapture;
        
        // Notify browser console for debugging
        RunJS($"console.log('[Unity WebGL] Keyboard capture set to {shouldCapture}');");
        
        // If using React Module compatibility, also expose WebGLInputHandler status
        if (enableReactModuleCompatibility)
        {
            string handlerCode = shouldCapture ? "window.unityOriginalInputHandler" : "null";
            RunJS($@"
                try {{
                    if (window.unityInstance && window.unityInstance.Module) {{
                        if (shouldCapture) {{
                            // Store the original handler if we haven't already
                            if (!window.unityOriginalInputHandler && window.unityInstance.Module.WebGLInputHandler) {{
                                window.unityOriginalInputHandler = window.unityInstance.Module.WebGLInputHandler;
                            }}
                            
                            // Restore handler if enabling capture
                            window.unityInstance.Module.WebGLInputHandler = window.unityOriginalInputHandler || window.unityInstance.Module.WebGLInputHandler;
                        }} else {{
                            // Store handler before disabling if we don't have it stored
                            if (!window.unityOriginalInputHandler && window.unityInstance.Module.WebGLInputHandler) {{
                                window.unityOriginalInputHandler = window.unityInstance.Module.WebGLInputHandler;
                            }}
                            
                            // Disable handler
                            window.unityInstance.Module.WebGLInputHandler = null;
                        }}
                        console.log('[Unity WebGL] Module.WebGLInputHandler ' + ({shouldCapture} ? 'enabled' : 'disabled'));
                    }}
                }} catch(e) {{
                    console.error('[Unity WebGL] Error updating Module.WebGLInputHandler:', e);
                }}
            ");
        }
        #endif
    }

    /// <summary>
    /// String overload for SendMessage (for easier JS interop)
    /// </summary>
    public void SetCaptureAllKeyboardInputString(string valueStr)
    {
        Debug.Log($"[WebGLInput] SetCaptureAllKeyboardInputString called with: {valueStr}");
        bool shouldCapture = valueStr.ToLower() == "true";
        SetCaptureAllKeyboardInput(shouldCapture);
    }

    /// <summary>
    /// Disables keyboard capture to let React handle input
    /// </summary>
    public void DisableKeyboardCapture()
    {
        Debug.Log("[WebGLInput] DisableKeyboardCapture called");
        SetCaptureAllKeyboardInput(false);
    }

    /// <summary>
    /// Enables keyboard capture so Unity can receive keyboard input
    /// </summary>
    public void EnableKeyboardCapture()
    {
        Debug.Log("[WebGLInput] EnableKeyboardCapture called");
        SetCaptureAllKeyboardInput(true);
    }

    /// <summary>
    /// Focus the Unity canvas
    /// </summary>
    public void FocusUnity()
    {
        Debug.Log("[WebGLInput] FocusUnity called");

        #if UNITY_WEBGL && !UNITY_EDITOR
        RunJS(@"
            try {
                var canvas = document.querySelector('canvas') || document.querySelector('#unity-canvas');
                if (canvas) {
                    if (!canvas.hasAttribute('tabindex')) {
                        canvas.setAttribute('tabindex', '1');
                    }
                    canvas.focus();
                    canvas.style.pointerEvents = 'auto';
                    console.log('[Unity WebGL] Canvas focused via FocusUnity() call');
                } else {
                    console.warn('[Unity WebGL] Canvas not found for focus');
                }
            } catch(e) {
                console.error('[Unity WebGL] Error focusing canvas:', e);
            }
        ");
        
        // Also enable keyboard capture
        EnableKeyboardCapture();
        #endif
    }

    /// <summary>
    /// Blur the Unity canvas (used by React)
    /// </summary>
    public void BlurUnity() 
    {
        Debug.Log("[WebGLInput] BlurUnity called");
        
        #if UNITY_WEBGL && !UNITY_EDITOR
        RunJS(@"
            try {
                var canvas = document.querySelector('canvas') || document.querySelector('#unity-canvas');
                if (canvas) {
                    canvas.blur();
                    canvas.style.pointerEvents = 'none';
                    console.log('[Unity WebGL] Canvas blurred via BlurUnity() call');
                }
            } catch(e) {
                console.error('[Unity WebGL] Error blurring canvas:', e);
            }
        ");
        
        // Also disable keyboard capture
        DisableKeyboardCapture();
        #endif
    }

    /// <summary>
    /// Public method that can be called directly from React to ensure JavaScript functions are set up
    /// </summary>
    public void SetupJavaScriptFunctions()
    {
        Debug.Log("[WebGLInput] SetupJavaScriptFunctions called from external source");
        
        #if UNITY_WEBGL && !UNITY_EDITOR
        // Set up the JavaScript functions
        InitializeJavaScriptFunctions();
        
        // Log confirmation that can be seen in browser console
        RunJS("console.log('[Unity WebGL] JavaScript functions initialized by explicit request at ' + new Date().toISOString());");
        #endif
    }
    
    /// <summary>
    /// Send a test message to verify communication with browser
    /// </summary>
    public void TestBrowserCommunication()
    {
        Debug.Log("[WebGLInput] Testing browser communication");
        
        #if UNITY_WEBGL && !UNITY_EDITOR
        RunJS(@"
            console.log('=== UNITY WEBGL COMMUNICATION TEST ===');
            console.log('Unity -> Browser communication working!');
            console.log('Unity WebGLInput controller is active');
            console.log('WebGLInput GameObject name: " + gameObject.name + @"');
            console.log('unityInstance available:', typeof window.unityInstance !== 'undefined');
            console.log('unityInstance.Module available:', typeof window.unityInstance?.Module !== 'undefined');
            console.log('WebGLInputHandler available:', typeof window.unityInstance?.Module?.WebGLInputHandler !== 'undefined');
            
            var canvas = document.querySelector('canvas') || document.querySelector('#unity-canvas');
            console.log('canvas element found:', canvas !== null);
            if (canvas) {
                console.log('canvas id:', canvas.id);
                console.log('canvas tabIndex:', canvas.getAttribute('tabindex'));
            }
            
            console.log('unityCaptureKeyboard function exists:', typeof window.unityCaptureKeyboard === 'function');
            console.log('unityFocus function exists:', typeof window.unityFocus === 'function');
            console.log('====================================');
        ");
        #endif
    }

    #if UNITY_WEBGL && !UNITY_EDITOR
    /// <summary>
    /// Sets up JavaScript functions for external control
    /// </summary>
    private void InitializeJavaScriptFunctions()
    {
        Debug.Log("[WebGLInput] Initializing JavaScript functions");
        
        RunJS(@"
            (function() {
                console.log('[Unity WebGL] Setting up input control functions');
                
                // Ensure the Unity canvas is focusable via tabIndex
                function setupCanvas() {
                    var canvas = document.querySelector('canvas') || document.querySelector('#unity-canvas');
                    if (canvas) {
                        if (!canvas.hasAttribute('tabindex')) {
                            canvas.setAttribute('tabindex', '1');
                            console.log('[Unity WebGL] Added tabindex=1 to canvas');
                        }
                        
                        // If canvas doesn't have an ID, assign one for easier selection
                        if (!canvas.id) {
                            canvas.id = 'unity-canvas';
                            console.log('[Unity WebGL] Assigned ID unity-canvas to canvas');
                        }
                        
                        return true;
                    }
                    console.warn('[Unity WebGL] Canvas not found during setup');
                    return false;
                }

                // Try immediately, or wait for canvas to be available
                if (!setupCanvas()) {
                    console.log('[Unity WebGL] Canvas not ready, will retry in 1s');
                    setTimeout(setupCanvas, 1000);
                }
                
                // Log if unityInstance isn't available yet
                if (typeof window.unityInstance === 'undefined') {
                    console.warn('[Unity WebGL] unityinstance not available yet, functions may not work until it is set');
                    
                    // Define a function to initialize once unityInstance becomes available
                    window.registerUnityInstance = function(instance) {
                        console.log('[Unity WebGL] registerUnityInstance called with instance:', !!instance);
                        
                        if (instance) {
                            window.unityInstance = instance;
                            console.log('[Unity WebGL] unityinstance set via registerUnityInstance');
                            
                            // Compatibility with React's WebGLInputHandler approach
                            if (instance.Module && instance.Module.WebGLInputHandler) {
                                window.unityOriginalInputHandler = instance.Module.WebGLInputHandler;
                                console.log('[Unity WebGL] Stored original WebGLInputHandler');
                            }
                            
                            // Re-initialize functions now that we have an instance
                            setTimeout(function() {
                                if (window.unityinstance && typeof window.unityinstance.SendMessage === 'function') {
                                    window.unityinstance.SendMessage('WebGLInput', 'SetupJavaScriptFunctions');
                                    console.log('[Unity WebGL] Re-initialized functions after receiving instance');
                                }
                            }, 500);
                        }
                    };
                    
                    console.log('[Unity WebGL] Defined registerUnityInstance function for late initialization');
                } else if (window.unityinstance.Module && window.unityinstance.Module.WebGLInputHandler) {
                    // Store original handler for React compatibility
                    window.unityOriginalInputHandler = window.unityinstance.Module.WebGLInputHandler;
                    console.log('[Unity WebGL] Stored original WebGLInputHandler');
                }
                
                // Global functions for React to control Unity input - returns Promises for compatibility
                window.unityCaptureKeyboard = function(capture) {
                    console.log('[Unity WebGL] unityCaptureKeyboard called with:', capture);
                    return new Promise((resolve, reject) => {
                        try {
                            if (window.unityinstance) {
                                window.unityinstance.SendMessage('WebGLInput', 'SetCaptureAllKeyboardInput', capture);
                                console.log('[Unity WebGL] Successfully sent keyboard capture command:', capture);
                                resolve(true);
                            } else {
                                console.warn('[Unity WebGL] unityinstance not found for keyboard capture');
                                
                                // Store the value to apply when unityinstance becomes available
                                window._pendingKeyboardCapture = capture;
                                console.log('[Unity WebGL] Stored pending keyboard capture value:', capture);
                                
                                reject(new Error('unityinstance not found'));
                            }
                        } catch (err) {
                            console.error('[Unity WebGL] Error in unityCaptureKeyboard:', err);
                            reject(err);
                        }
                    });
                };
                
                window.unityFocus = function() {
                    console.log('[Unity WebGL] unityFocus called');
                    return new Promise((resolve, reject) => {
                        try {
                            if (window.unityinstance) {
                                window.unityinstance.SendMessage('WebGLInput', 'FocusUnity');
                                console.log('[Unity WebGL] Successfully sent focus command');
                                resolve(true);
                            } else {
                                console.warn('[Unity WebGL] unityinstance not found for focus');
                                
                                // Store focus request for when unityinstance is available
                                window._pendingFocusRequest = true;
                                console.log('[Unity WebGL] Stored pending focus request');
                                
                                reject(new Error('unityinstance not found'));
                            }
                        } catch (err) {
                            console.error('[Unity WebGL] Error in unityFocus:', err);
                            reject(err);
                        }
                    });
                };
                
                // New function for blurring Unity 
                window.unityBlur = function() {
                    console.log('[Unity WebGL] unityBlur called');
                    return new Promise((resolve, reject) => {
                        try {
                            if (window.unityinstance) {
                                window.unityinstance.SendMessage('WebGLInput', 'BlurUnity');
                                console.log('[Unity WebGL] Successfully sent blur command');
                                resolve(true);
                            } else {
                                console.warn('[Unity WebGL] unityinstance not found for blur');
                                reject(new Error('unityinstance not found'));
                            }
                        } catch (err) {
                            console.error('[Unity WebGL] Error in unityBlur:', err);
                            reject(err);
                        }
                    });
                };
                
                // Add direct handlers for React's Module.WebGLInputHandler approach
                window.disableUnityKeyboardHandler = function() {
                    if (window.unityinstance && window.unityinstance.Module) {
                        // Store original if not already stored
                        if (!window.unityOriginalInputHandler && window.unityinstance.Module.WebGLInputHandler) {
                            window.unityOriginalInputHandler = window.unityinstance.Module.WebGLInputHandler;
                        }
                        
                        // Disable handler
                        window.unityinstance.Module.WebGLInputHandler = null;
                        console.log('[Unity WebGL] Disabled WebGLInputHandler');
                        return true;
                    }
                    return false;
                };
                
                window.restoreUnityKeyboardHandler = function() {
                    if (window.unityinstance && window.unityinstance.Module && window.unityOriginalInputHandler) {
                        window.unityinstance.Module.WebGLInputHandler = window.unityOriginalInputHandler;
                        console.log('[Unity WebGL] Restored WebGLInputHandler');
                        return true;
                    }
                    return false;
                };
                
                // Add direct keyboard capture setter as a fallback
                window.unityCaptureKeyboardSync = function(capture) {
                    console.log('[Unity WebGL] unityCaptureKeyboardSync called with:', capture);
                    if (window.unityinstance) {
                        window.unityinstance.SendMessage('WebGLInput', 'SetCaptureAllKeyboardInput', capture);
                        return true;
                    } else {
                        console.warn('[Unity WebGL] unityinstance not found (sync call)');
                        
                        // Store the value to apply when unityinstance becomes available
                        window._pendingKeyboardCapture = capture;
                        console.log('[Unity WebGL] Stored pending keyboard capture value:', capture);
                        
                        return false;
                    }
                };
                
                console.log('[Unity WebGL] Input control functions initialized with Promise support');
            })();
        ");
        
        // Add a delayed check to verify if functions were properly set up
        RunJS(@"
            setTimeout(function() {
                console.log('[Unity WebGL] Verification check for input functions:');
                console.log('- unityCaptureKeyboard exists:', typeof window.unityCaptureKeyboard === 'function');
                console.log('- unityFocus exists:', typeof window.unityFocus === 'function');
                console.log('- unityBlur exists:', typeof window.unityBlur === 'function');
                console.log('- disableUnityKeyboardHandler exists:', typeof window.disableUnityKeyboardHandler === 'function');
                console.log('- restoreUnityKeyboardHandler exists:', typeof window.restoreUnityKeyboardHandler === 'function');
                console.log('- unityCaptureKeyboardSync exists:', typeof window.unityCaptureKeyboardSync === 'function');
                console.log('- unityinstance exists:', typeof window.unityinstance !== 'undefined');
                console.log('- registerUnityInstance exists:', typeof window.registerUnityInstance === 'function');
                console.log('- unityOriginalInputHandler exists:', typeof window.unityOriginalInputHandler !== 'undefined');
                
                // Check if we need to apply any pending operations
                if (window.unityinstance) {
                    if (window._pendingKeyboardCapture !== undefined) {
                        console.log('[Unity WebGL] Applying pending keyboard capture:', window._pendingKeyboardCapture);
                        window.unityCaptureKeyboardSync(window._pendingKeyboardCapture);
                        delete window._pendingKeyboardCapture;
                    }
                    
                    if (window._pendingFocusRequest) {
                        console.log('[Unity WebGL] Applying pending focus request');
                        window.unityFocus();
                        delete window._pendingFocusRequest;
                    }
                }
            }, 2000);
        ");
    }

    /// <summary>
    /// Helper to run JavaScript with error handling
    /// </summary>
    private void RunJS(string jsCode)
    {
        try {
            RunJavaScript(jsCode);
        } catch (System.Exception e) {
            Debug.LogError($"[WebGLInput] Error running JavaScript: {e.Message}");
        }
    }
    #endif

    /// <summary>
    /// Helper for conditional logging
    /// </summary>
    private void Log(string message)
    {
        if (debugLogging)
        {
            Debug.Log($"[WebGLInput] {message}");
        }
    }
    
    private void OnEnable()
    {
        Debug.Log("[WebGLInput] WebGLInputController enabled");
    }
    
    private void OnDisable()
    {
        Debug.Log("[WebGLInput] WebGLInputController disabled");
    }
    
    private void OnDestroy()
    {
        Debug.LogWarning("[WebGLInput] WebGLInputController was destroyed! This may cause keyboard input issues.");
    }
} 
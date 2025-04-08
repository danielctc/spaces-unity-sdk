using UnityEngine;
using System.Runtime.InteropServices;

/// <summary>
/// Unity WebGL input manager with React compatibility
/// </summary>
public class ReactKeyboardManager : MonoBehaviour
{
    [SerializeField] private bool debugLogging = true;
    [SerializeField] private bool startWithKeyboardCaptureDisabled = false;
    [SerializeField] private bool setupTabIndexOnCanvas = true;
    
    #if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void RunJavaScript(string jsCode);
    #endif
    
    private void Awake()
    {
        // Ensure object name is consistent for SendMessage calls
        if (gameObject.name != "WebGLInput")
        {
            gameObject.name = "WebGLInput";
            Debug.Log("[ReactKeyboard] Set GameObject name to 'WebGLInput'");
        }
        
        DontDestroyOnLoad(gameObject);
    }
    
    private void Start()
    {
        Debug.Log("[ReactKeyboard] Initializing");
        
        #if UNITY_WEBGL && !UNITY_EDITOR
        // Initialize default state
        WebGLInput.captureAllKeyboardInput = !startWithKeyboardCaptureDisabled;
        Debug.Log($"[ReactKeyboard] Initial keyboard capture set to {!startWithKeyboardCaptureDisabled}");
        
        // Add React compatibility
        InitializeJavaScript();
        #endif
    }
    
    /// <summary>
    /// Called via SendMessage to enable/disable keyboard input capture
    /// </summary>
    public void SetCaptureAllKeyboardInput(bool shouldCapture)
    {
        Debug.Log($"[ReactKeyboard] SetCaptureAllKeyboardInput({shouldCapture})");
        
        #if UNITY_WEBGL && !UNITY_EDITOR
        WebGLInput.captureAllKeyboardInput = shouldCapture;
        
        // Also update the Module.WebGLInputHandler for React compatibility
        string js = $@"
            try {{
                console.log('[Unity] Setting keyboard capture to {shouldCapture}');
                
                if (window.unityInstance && window.unityInstance.Module) {{
                    if ({(shouldCapture ? "true" : "false")}) {{
                        // Restore handler
                        if (window._originalWebGLInputHandler) {{
                            window.unityInstance.Module.WebGLInputHandler = window._originalWebGLInputHandler;
                            console.log('[Unity] Restored WebGLInputHandler');
                        }}
                    }} else {{
                        // Store handler if we don't have it yet
                        if (!window._originalWebGLInputHandler && 
                            window.unityInstance.Module.WebGLInputHandler) {{
                            window._originalWebGLInputHandler = window.unityInstance.Module.WebGLInputHandler;
                            console.log('[Unity] Saved original WebGLInputHandler');
                        }}
                        
                        // Disable handler
                        window.unityInstance.Module.WebGLInputHandler = null;
                        console.log('[Unity] Disabled WebGLInputHandler');
                    }}
                }}
            }} catch (e) {{
                console.error('[Unity] Error setting keyboard capture:', e);
            }}
        ";
        
        RunJS(js);
        #endif
    }
    
    /// <summary>
    /// String overload for SendMessage from JavaScript
    /// </summary>
    public void SetCaptureAllKeyboardInputString(string valueStr)
    {
        bool value = valueStr.ToLower() == "true";
        SetCaptureAllKeyboardInput(value);
    }
    
    /// <summary>
    /// Directly disables keyboard capture
    /// </summary>
    public void DisableKeyboardCapture()
    {
        Debug.Log("[ReactKeyboard] DisableKeyboardCapture()");
        SetCaptureAllKeyboardInput(false);
    }
    
    /// <summary>
    /// Directly enables keyboard capture
    /// </summary>
    public void EnableKeyboardCapture()
    {
        Debug.Log("[ReactKeyboard] EnableKeyboardCapture()");
        SetCaptureAllKeyboardInput(true);
    }
    
    /// <summary>
    /// Sets focus to Unity canvas and enables keyboard input
    /// </summary>
    public void FocusUnity()
    {
        Debug.Log("[ReactKeyboard] FocusUnity()");
        
        #if UNITY_WEBGL && !UNITY_EDITOR
        string js = @"
            try {
                var canvas = document.querySelector('#unity-canvas') || document.querySelector('canvas');
                if (canvas) {
                    // Ensure tabindex exists
                    if (!canvas.hasAttribute('tabindex')) {
                        canvas.setAttribute('tabindex', '1');
                    }
                    
                    // Focus and enable pointer events
                    canvas.focus();
                    canvas.style.pointerEvents = 'auto';
                    console.log('[Unity] Canvas focused');
                } else {
                    console.warn('[Unity] Could not find canvas to focus');
                }
            } catch (e) {
                console.error('[Unity] Error focusing canvas:', e);
            }
        ";
        
        RunJS(js);
        EnableKeyboardCapture();
        #endif
    }
    
    /// <summary>
    /// Removes focus from Unity canvas and disables keyboard input
    /// </summary>
    public void BlurUnity()
    {
        Debug.Log("[ReactKeyboard] BlurUnity()");
        
        #if UNITY_WEBGL && !UNITY_EDITOR
        string js = @"
            try {
                var canvas = document.querySelector('#unity-canvas') || document.querySelector('canvas');
                if (canvas) {
                    canvas.blur();
                    canvas.style.pointerEvents = 'none';
                    console.log('[Unity] Canvas blurred');
                }
            } catch (e) {
                console.error('[Unity] Error blurring canvas:', e);
            }
        ";
        
        RunJS(js);
        DisableKeyboardCapture();
        #endif
    }
    
    /// <summary>
    /// Initialize JavaScript helper functions for React
    /// </summary>
    private void InitializeJavaScript()
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
        Debug.Log("[ReactKeyboard] Setting up JavaScript helpers");
        
        string js = @"
            (function() {
                console.log('[Unity] Setting up keyboard integration with React');
                
                // Set up canvas with tabindex if needed
                function setupCanvas() {
                    var canvas = document.querySelector('#unity-canvas') || document.querySelector('canvas');
                    if (canvas) {
                        // Set ID if not present
                        if (!canvas.id) {
                            canvas.id = 'unity-canvas';
                        }
                        
                        // Set tabindex if not present
                        if (!canvas.hasAttribute('tabindex')) {
                            canvas.setAttribute('tabindex', '1');
                        }
                        
                        console.log('[Unity] Canvas setup complete, id:', canvas.id);
                        return true;
                    }
                    
                    console.warn('[Unity] Canvas not found, will retry shortly');
                    return false;
                }
                
                // Try to set up canvas immediately, or retry after delay
                if (!setupCanvas()) {
                    setTimeout(setupCanvas, 1000);
                }
                
                // Helper function: React to Unity (disable keyboard capture)
                window.unityDisableKeyboard = function() {
                    console.log('[Unity] unityDisableKeyboard called');
                    
                    try {
                        if (window.unityInstance) {
                            // Direct method
                            window.unityInstance.SendMessage('WebGLInput', 'DisableKeyboardCapture');
                            return Promise.resolve(true);
                        } else {
                            console.warn('[Unity] unityInstance not available yet');
                            
                            // Store for when Unity is ready
                            window._pendingKeyboardDisable = true;
                            
                            return Promise.reject(new Error('Unity not initialized'));
                        }
                    } catch (e) {
                        console.error('[Unity] Error disabling keyboard:', e);
                        return Promise.reject(e);
                    }
                };
                
                // Helper function: React to Unity (enable keyboard capture)
                window.unityEnableKeyboard = function() {
                    console.log('[Unity] unityEnableKeyboard called');
                    
                    try {
                        if (window.unityInstance) {
                            // Direct method
                            window.unityInstance.SendMessage('WebGLInput', 'EnableKeyboardCapture');
                            return Promise.resolve(true);
                        } else {
                            console.warn('[Unity] unityInstance not available yet');
                            
                            // Store for when Unity is ready
                            window._pendingKeyboardEnable = true;
                            
                            return Promise.reject(new Error('Unity not initialized'));
                        }
                    } catch (e) {
                        console.error('[Unity] Error enabling keyboard:', e);
                        return Promise.reject(e);
                    }
                };
                
                // For compatibility with Module.WebGLInputHandler approach
                window.unityDisableKeyboardHandler = function() {
                    console.log('[Unity] unityDisableKeyboardHandler called');
                    
                    try {
                        if (window.unityInstance && window.unityInstance.Module) {
                            // Store original handler if we don't have it yet
                            if (!window._originalWebGLInputHandler && window.unityInstance.Module.WebGLInputHandler) {
                                window._originalWebGLInputHandler = window.unityInstance.Module.WebGLInputHandler;
                            }
                            
                            // Disable handler
                            window.unityInstance.Module.WebGLInputHandler = null;
                            return true;
                        }
                        return false;
                    } catch (e) {
                        console.error('[Unity] Error disabling keyboard handler:', e);
                        return false;
                    }
                };
                
                // For compatibility with Module.WebGLInputHandler approach
                window.unityRestoreKeyboardHandler = function() {
                    console.log('[Unity] unityRestoreKeyboardHandler called');
                    
                    try {
                        if (window.unityInstance && window.unityInstance.Module && window._originalWebGLInputHandler) {
                            window.unityInstance.Module.WebGLInputHandler = window._originalWebGLInputHandler;
                            return true;
                        }
                        return false;
                    } catch (e) {
                        console.error('[Unity] Error restoring keyboard handler:', e);
                        return false;
                    }
                };
                
                // When the Unity instance is set in React, handle any pending operations
                var originalUnityInstance = window.unityInstance;
                Object.defineProperty(window, 'unityInstance', {
                    set: function(value) {
                        console.log('[Unity] unityInstance property set');
                        originalUnityInstance = value;
                        
                        // Handle any pending operations
                        setTimeout(function() {
                            if (window._pendingKeyboardDisable) {
                                console.log('[Unity] Applying pending keyboard disable');
                                window.unityDisableKeyboard();
                                delete window._pendingKeyboardDisable;
                            }
                            
                            if (window._pendingKeyboardEnable) {
                                console.log('[Unity] Applying pending keyboard enable');
                                window.unityEnableKeyboard();
                                delete window._pendingKeyboardEnable;
                            }
                        }, 500);
                    },
                    get: function() {
                        return originalUnityInstance;
                    }
                });
                
                console.log('[Unity] Keyboard integration setup complete');
            })();
        ";
        
        RunJS(js);
        #endif
    }
    
    #if UNITY_WEBGL && !UNITY_EDITOR
    /// <summary>
    /// Helper to safely run JavaScript
    /// </summary>
    private void RunJS(string js)
    {
        try {
            RunJavaScript(js);
        } catch (System.Exception e) {
            Debug.LogError($"[ReactKeyboard] Error running JavaScript: {e.Message}");
        }
    }
    #endif
    
    /// <summary>
    /// Log helper with conditional output
    /// </summary>
    private void Log(string message)
    {
        if (debugLogging)
        {
            Debug.Log($"[ReactKeyboard] {message}");
        }
    }
} 
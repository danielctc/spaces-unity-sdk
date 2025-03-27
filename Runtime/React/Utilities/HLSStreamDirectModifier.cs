using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Spaces.React.Runtime
{
    /// <summary>
    /// Direct modifier for HISPlayer stream URL using a targeted approach.
    /// This script focuses exclusively on setting the URL at multiStreamProperties[0].url[0]
    /// </summary>
    [AddComponentMenu("Spaces/React/HLS Stream Direct Modifier")]
    public class HLSStreamDirectModifier : MonoBehaviour
    {
        [Tooltip("The HISPlayerController to modify")]
        public MonoBehaviour playerController;
        
        [Tooltip("The URL to set")]
        public string streamUrl = "https://app.viloud.tv/hls/channel/67951f3e3286f823aa88edab9bf2713b.m3u8";
        
        [Tooltip("Whether to set the URL on start")]
        public bool setOnStart = true;
        
        [Tooltip("Delay in seconds before setting URL")]
        [Range(0.1f, 5.0f)]
        public float initialDelay = 1.0f;
        
        [Tooltip("If true, logs detailed information about the structure")]
        public bool debugMode = true;
        
        private void Start()
        {
            if (setOnStart && !string.IsNullOrEmpty(streamUrl))
            {
                StartCoroutine(DelayedURLSet());
            }
        }
        
        private IEnumerator DelayedURLSet()
        {
            if (debugMode)
            {
                Debug.Log($"HLSStreamDirectModifier: Waiting {initialDelay} seconds before setting URL...");
            }
            
            yield return new WaitForSeconds(initialDelay);
            
            if (debugMode)
            {
                InspectStructure();
            }
            
            SetURL(streamUrl);
        }
        
        /// <summary>
        /// Set the URL for the stream directly at multiStreamProperties[0].url[0]
        /// </summary>
        public void SetURL(string url)
        {
            if (playerController == null)
            {
                Debug.Log("HLSStreamDirectModifier: No player controller assigned!");
                return;
            }
            
            if (string.IsNullOrEmpty(url))
            {
                Debug.Log("HLSStreamDirectModifier: URL is empty!");
                return;
            }
            
            Debug.Log($"HLSStreamDirectModifier: Setting URL to: {url}");
            
            try
            {
                // Get the multiStreamProperties field
                FieldInfo multiStreamPropsField = playerController.GetType().GetField("multiStreamProperties", 
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
                
                if (multiStreamPropsField == null)
                {
                    Debug.Log("HLSStreamDirectModifier: Could not find multiStreamProperties field");
                    return;
                }
                
                // Get the value of the field (should be a list/array)
                object multiStreamProps = multiStreamPropsField.GetValue(playerController);
                if (multiStreamProps == null)
                {
                    Debug.Log("HLSStreamDirectModifier: multiStreamProperties is null");
                    return;
                }
                
                // Get the Count property
                PropertyInfo countProp = multiStreamProps.GetType().GetProperty("Count");
                if (countProp == null)
                {
                    Debug.Log("HLSStreamDirectModifier: Could not find Count property on multiStreamProperties");
                    return;
                }
                
                // Get the count value
                int count = (int)countProp.GetValue(multiStreamProps);
                if (count <= 0)
                {
                    Debug.Log("HLSStreamDirectModifier: multiStreamProperties is empty");
                    
                    // Try to add a new element
                    TryAddNewStreamElement(multiStreamProps, url);
                    return;
                }
                
                // Get the indexer method
                MethodInfo indexerMethod = multiStreamProps.GetType().GetMethod("get_Item");
                if (indexerMethod == null)
                {
                    Debug.Log("HLSStreamDirectModifier: Could not find get_Item method on multiStreamProperties");
                    return;
                }
                
                // Get the first element (index 0)
                object streamElement = null;
                try {
                    streamElement = indexerMethod.Invoke(multiStreamProps, new object[] { 0 });
                } catch (System.Exception e) {
                    Debug.Log($"HLSStreamDirectModifier: Error getting first stream element: {e.Message}");
                    return;
                }
                
                if (streamElement == null)
                {
                    Debug.Log("HLSStreamDirectModifier: multiStreamProperties[0] is null");
                    return;
                }
                
                // Find the url field in the stream element
                FieldInfo urlField = streamElement.GetType().GetField("url", 
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
                
                if (urlField == null)
                {
                    Debug.Log("HLSStreamDirectModifier: Could not find url field in stream element");
                    return;
                }
                
                // Get the url list
                object urlList = urlField.GetValue(streamElement);
                
                // If the list is null, create a new one
                if (urlList == null)
                {
                    Debug.Log("HLSStreamDirectModifier: Creating new URL list");
                    urlList = System.Activator.CreateInstance(typeof(List<string>));
                    urlField.SetValue(streamElement, urlList);
                }
                
                // Get the Clear method
                MethodInfo clearMethod = urlList.GetType().GetMethod("Clear");
                if (clearMethod == null)
                {
                    Debug.Log("HLSStreamDirectModifier: Could not find Clear method on URL list");
                    return;
                }
                
                // Clear the list
                try {
                    clearMethod.Invoke(urlList, null);
                } catch (System.Exception e) {
                    Debug.Log($"HLSStreamDirectModifier: Error clearing URL list: {e.Message}");
                }
                
                // Get the Add method
                MethodInfo addMethod = urlList.GetType().GetMethod("Add", new[] { typeof(string) });
                if (addMethod == null)
                {
                    Debug.Log("HLSStreamDirectModifier: Could not find Add method on URL list");
                    return;
                }
                
                // Add the URL
                try {
                    addMethod.Invoke(urlList, new object[] { url });
                    Debug.Log("HLSStreamDirectModifier: Successfully set URL at multiStreamProperties[0].url[0]");
                } catch (System.Exception e) {
                    Debug.Log($"HLSStreamDirectModifier: Error adding URL to list: {e.Message}");
                    return;
                }
                
                // Force refresh
                ForceRefresh();
                
                if (debugMode)
                {
                    // Verify the structure again after setting
                    Invoke("InspectStructure", 0.5f);
                }
            }
            catch (System.Exception e)
            {
                Debug.Log($"HLSStreamDirectModifier: Error setting URL: {e.Message}");
            }
        }
        
        private void TryAddNewStreamElement(object multiStreamProps, string url)
        {
            try
            {
                Debug.Log("HLSStreamDirectModifier: Attempting to add a new stream element");
                
                // Find the Add method
                MethodInfo addMethod = multiStreamProps.GetType().GetMethod("Add");
                if (addMethod == null)
                {
                    Debug.Log("HLSStreamDirectModifier: Could not find Add method on multiStreamProperties");
                    return;
                }
                
                // Try to create a new stream element
                // First, try to find the type of the element
                PropertyInfo itemTypeProp = multiStreamProps.GetType().GetProperty("Item");
                if (itemTypeProp == null)
                {
                    Debug.Log("HLSStreamDirectModifier: Could not determine element type");
                    return;
                }
                
                // Get the type
                System.Type elementType = itemTypeProp.PropertyType;
                
                // Create a new instance
                object newElement = null;
                try {
                    newElement = System.Activator.CreateInstance(elementType);
                } catch (System.Exception e) {
                    Debug.Log($"HLSStreamDirectModifier: Error creating new element instance: {e.Message}");
                    return;
                }
                
                // Find the url field
                FieldInfo urlField = elementType.GetField("url", BindingFlags.Public | BindingFlags.Instance);
                if (urlField == null)
                {
                    Debug.Log("HLSStreamDirectModifier: Could not find url field in element type");
                    return;
                }
                
                // Create a new url list
                var urlList = new List<string> { url };
                
                // Set the url field
                try {
                    urlField.SetValue(newElement, urlList);
                } catch (System.Exception e) {
                    Debug.Log($"HLSStreamDirectModifier: Error setting URL field: {e.Message}");
                    return;
                }
                
                // Add the element to the collection
                try {
                    addMethod.Invoke(multiStreamProps, new object[] { newElement });
                    Debug.Log("HLSStreamDirectModifier: Successfully added new stream element with URL");
                } catch (System.Exception e) {
                    Debug.Log($"HLSStreamDirectModifier: Error adding new element to collection: {e.Message}");
                }
            }
            catch (System.Exception e)
            {
                Debug.Log($"HLSStreamDirectModifier: Error adding stream element: {e.Message}");
            }
        }
        
        private void ForceRefresh()
        {
            try
            {
                // Attempt to call Refresh or SetUpPlayer
                MethodInfo refreshMethod = playerController.GetType().GetMethod("Refresh", 
                    BindingFlags.Public | BindingFlags.Instance);
                
                if (refreshMethod != null)
                {
                    try {
                        refreshMethod.Invoke(playerController, null);
                        Debug.Log("HLSStreamDirectModifier: Called Refresh method");
                        return;
                    }
                    catch (System.Exception innerEx) {
                        Debug.Log($"HLSStreamDirectModifier: Refresh method exists but couldn't be called: {innerEx.Message}");
                        // Continue to next approach instead of failing
                    }
                }
                
                // Try calling Setup or Update
                MethodInfo setupMethod = playerController.GetType().GetMethod("SetUpPlayer", 
                    BindingFlags.Public | BindingFlags.Instance);
                
                if (setupMethod != null)
                {
                    int paramCount = setupMethod.GetParameters().Length;
                    if (paramCount == 0)
                    {
                        try {
                            setupMethod.Invoke(playerController, null);
                            Debug.Log("HLSStreamDirectModifier: Called SetUpPlayer method");
                            return;
                        }
                        catch (System.Exception innerEx) {
                            Debug.Log($"HLSStreamDirectModifier: SetUpPlayer method exists but couldn't be called: {innerEx.Message}");
                            // Continue to next approach instead of failing
                        }
                    }
                }
                
                // Try OnNextPlayback/OnPreviousPlayback to cycle
                MethodInfo nextMethod = playerController.GetType().GetMethod("OnNextPlayback", 
                    BindingFlags.Public | BindingFlags.Instance);
                
                if (nextMethod != null)
                {
                    try {
                        nextMethod.Invoke(playerController, new object[] { 0 });
                        Debug.Log("HLSStreamDirectModifier: Called OnNextPlayback method");
                        
                        // Wait a bit and then go back
                        Invoke("CallPreviousPlayback", 0.1f);
                    }
                    catch (System.Exception innerEx) {
                        Debug.Log($"HLSStreamDirectModifier: OnNextPlayback method exists but couldn't be called: {innerEx.Message}");
                    }
                }
                
                // If we reached here, none of the methods worked, but we don't need to throw an error
                Debug.Log("HLSStreamDirectModifier: None of the refresh methods succeeded, but URL was still set correctly");
            }
            catch (System.Exception e)
            {
                // This is a general error in our code, not in the method invocation
                Debug.Log($"HLSStreamDirectModifier: Error in refresh method implementation: {e.Message}");
            }
        }
        
        private void CallPreviousPlayback()
        {
            try
            {
                MethodInfo prevMethod = playerController.GetType().GetMethod("OnPreviousPlayback", 
                    BindingFlags.Public | BindingFlags.Instance);
                
                if (prevMethod != null)
                {
                    try {
                        prevMethod.Invoke(playerController, new object[] { 0 });
                        Debug.Log("HLSStreamDirectModifier: Called OnPreviousPlayback method");
                    }
                    catch (System.Exception innerEx) {
                        Debug.Log($"HLSStreamDirectModifier: OnPreviousPlayback method exists but couldn't be called: {innerEx.Message}");
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.Log($"HLSStreamDirectModifier: Error in previous playback helper: {e.Message}");
            }
        }
        
        private void InspectStructure()
        {
            if (playerController == null)
            {
                Debug.Log("HLSStreamDirectModifier: No player controller assigned for inspection");
                return;
            }
            
            Debug.Log($"HLSStreamDirectModifier: Inspecting player controller of type {playerController.GetType().FullName}");
            
            try
            {
                // Check if multiStreamProperties exists
                FieldInfo multiStreamPropsField = playerController.GetType().GetField("multiStreamProperties", 
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
                
                if (multiStreamPropsField == null)
                {
                    Debug.Log("HLSStreamDirectModifier: Could not find multiStreamProperties field during inspection");
                    return;
                }
                
                object multiStreamProps = multiStreamPropsField.GetValue(playerController);
                if (multiStreamProps == null)
                {
                    Debug.Log("HLSStreamDirectModifier: multiStreamProperties is null during inspection");
                    return;
                }
                
                Debug.Log($"HLSStreamDirectModifier: multiStreamProperties is of type {multiStreamProps.GetType().FullName}");
                
                // Get the Count property
                PropertyInfo countProp = multiStreamProps.GetType().GetProperty("Count");
                if (countProp == null)
                {
                    Debug.Log("HLSStreamDirectModifier: Could not find Count property during inspection");
                    return;
                }
                
                int count = (int)countProp.GetValue(multiStreamProps);
                Debug.Log($"HLSStreamDirectModifier: multiStreamProperties has {count} elements");
                
                if (count <= 0)
                {
                    Debug.Log("HLSStreamDirectModifier: multiStreamProperties is empty during inspection");
                    return;
                }
                
                // Get the indexer method
                MethodInfo indexerMethod = multiStreamProps.GetType().GetMethod("get_Item");
                if (indexerMethod == null)
                {
                    Debug.Log("HLSStreamDirectModifier: Could not find get_Item method during inspection");
                    return;
                }
                
                // Get the first element
                object streamElement = indexerMethod.Invoke(multiStreamProps, new object[] { 0 });
                if (streamElement == null)
                {
                    Debug.Log("HLSStreamDirectModifier: multiStreamProperties[0] is null during inspection");
                    return;
                }
                
                Debug.Log($"HLSStreamDirectModifier: multiStreamProperties[0] is of type {streamElement.GetType().FullName}");
                
                // Get the url field
                FieldInfo urlField = streamElement.GetType().GetField("url", 
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
                
                if (urlField == null)
                {
                    Debug.Log("HLSStreamDirectModifier: Could not find url field during inspection");
                    return;
                }
                
                object urlList = urlField.GetValue(streamElement);
                if (urlList == null)
                {
                    Debug.Log("HLSStreamDirectModifier: url list is null during inspection");
                    return;
                }
                
                Debug.Log($"HLSStreamDirectModifier: url list is of type {urlList.GetType().FullName}");
                
                // Get the Count property for the url list
                PropertyInfo urlCountProp = urlList.GetType().GetProperty("Count");
                if (urlCountProp == null)
                {
                    Debug.Log("HLSStreamDirectModifier: Could not find Count property on url list during inspection");
                    return;
                }
                
                int urlCount = (int)urlCountProp.GetValue(urlList);
                Debug.Log($"HLSStreamDirectModifier: url list has {urlCount} elements");
                
                if (urlCount <= 0)
                {
                    Debug.Log("HLSStreamDirectModifier: url list is empty during inspection");
                    return;
                }
                
                // Get the url at index 0
                MethodInfo urlIndexerMethod = urlList.GetType().GetMethod("get_Item");
                if (urlIndexerMethod == null)
                {
                    Debug.Log("HLSStreamDirectModifier: Could not find get_Item method on url list during inspection");
                    return;
                }
                
                object url = urlIndexerMethod.Invoke(urlList, new object[] { 0 });
                Debug.Log($"HLSStreamDirectModifier: multiStreamProperties[0].url[0] = {url}");
            }
            catch (System.Exception e)
            {
                Debug.Log($"HLSStreamDirectModifier: Error during inspection: {e.Message}");
            }
        }
        
        /// <summary>
        /// Public method to set a new URL and force a refresh
        /// </summary>
        public void UpdateURL(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                Debug.LogError("HLSStreamDirectModifier: Cannot update with empty URL");
                return;
            }
            
            streamUrl = url;
            SetURL(url);
        }
    }
} 
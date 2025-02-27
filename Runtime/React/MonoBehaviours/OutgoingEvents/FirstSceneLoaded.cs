using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class FirstSceneLoaded : MonoBehaviour
{
    IEnumerator Start()
    {
        Debug.Log("Showing splash screen");
        while (!SplashScreen.isFinished)
        {
            Debug.Log("Waiting for splash screen to finish...");
            yield return null;
        }
        Debug.Log("Finished showing splash screen");

        // Pass the example data to the RaiseHelloReact method
        ReactRaiseEvent.FirstSceneLoaded();
    }
}

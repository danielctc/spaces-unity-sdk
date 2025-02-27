using System.Runtime.InteropServices;
using UnityEngine;
using GameCreator.Runtime.VisualScripting;
using Spaces.React.Runtime;

namespace Spaces.React.Runtime
{


public class TriggerEmote : MonoBehaviour
{
    // Reference to your EmoteTest GameObject that contains the Trigger component.
    public GameObject EmoteTest;
    private Trigger emoteTrigger;  // Store the reference to the Game Creator Trigger component.

    private void OnEnable()
    {
        ReactIncomingEvent.OnReactEmoteTest += HandleReactEmoteTest;

    }

    private void OnDisable()
    {
        ReactIncomingEvent.OnReactEmoteTest -= HandleReactEmoteTest;
    }

    private void Start()
    {
        // Ensure the emoteTest GameObject has a Trigger component.
        if (EmoteTest != null)
        {
            emoteTrigger = EmoteTest.GetComponent<Trigger>();
            if (emoteTrigger == null)
            {
                Debug.LogError("Trigger component not found on the emoteTest object!");
            }
        }
    }

    // Call this function from JS when the React button is pressed.
    public void HandleReactEmoteTest(EmoteTestData data)
    {
        Debug.Log("Unity: Received message to execute EmoteTest");

        // Check if emoteTrigger is available and call the Invoke method.
        if (emoteTrigger != null)
        {
            emoteTrigger.Invoke();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // ExecuteEmoteFromJS();
            EmoteTestData emoteTestData = new EmoteTestData();
            emoteTestData.emoteType = "Happy";

            HandleReactEmoteTest(emoteTestData);
        }
    }


}
}

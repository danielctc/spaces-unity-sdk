using TMPro;
using UnityEngine;
using Spaces.React.Runtime;

namespace Spaces.React.Runtime
{


public class HelloFromReact : MonoBehaviour
{
    private TMP_Text displayText;

    private void Start()
    {
        displayText = GetComponentInChildren<TMP_Text>();
        if (displayText == null)
        {
            Debug.LogError("TextMeshPro component not found in children!");
        }
    }

    private void OnEnable()
    {
        ReactIncomingEvent.OnReactHelloFromReact += HandleHelloFromReact;

    }

    private void OnDisable()
    {
        ReactIncomingEvent.OnReactHelloFromReact -= HandleHelloFromReact;
    }

    private void HandleHelloFromReact(HelloFromReactData data)
    {
        Debug.Log("Received HelloFromReact: " + data.name + ", React Age: " + data.reactAge);
        if (displayText)
        {
            displayText.SetText("Name - " + data.name + ", Age - " + data.reactAge);
        }
    }
}
}

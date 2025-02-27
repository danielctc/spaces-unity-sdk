using UnityEngine;
using Spaces.React.Runtime;

namespace Spaces.React.Runtime
{

public class ReceivedFirebaseUser : MonoBehaviour
{

    // Example script that listens for a firebase user being sent from React and logs debug info.

    private void Start()
    {

    }

    private void OnEnable()
    {
        ReactIncomingEvent.OnReceivedFirebaseUser += HandleOnReceivedFirebaseUser;

    }

    private void OnDisable()
    {
        ReactIncomingEvent.OnReceivedFirebaseUser -= HandleOnReceivedFirebaseUser;
    }

    private void HandleOnReceivedFirebaseUser(FirebaseUserData data)
    {
        Debug.Log("User: Assets/Spaces SDK/Scripts/React/MonoBehaviours/IncomingEvents/RecievedFirebaseUser.cs: Received User from react uid: " + data.uid + ", email: " + data.email);
    }
}
}

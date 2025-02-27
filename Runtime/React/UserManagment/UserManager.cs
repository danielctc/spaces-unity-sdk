using UnityEngine;
using System.Runtime.InteropServices;
using Spaces.React.Runtime;

namespace Spaces.React.Runtime
{
public static class UserManager
{
    private static FirebaseUserData currentUser;
    public static FirebaseUserData CurrentUser => currentUser;

    // Static constructor
    public static void InitailizeManually()
    {
        Initialize();
    }

    private static void Initialize()
    {
        // Subscribe to the event
        ReactIncomingEvent.OnReceivedFirebaseUser += HandleOnReceivedFirebaseUser;
    }

    // Static method to handle the event
    private static void HandleOnReceivedFirebaseUser(FirebaseUserData user)
    {
        currentUser = user;
        Debug.Log($"UserManager: Received Firebase User: {user.Nickname}");
    }

    // Method to safely unsubscribe from the event when it's no longer needed or before application quits
    public static void Cleanup()
    {
        ReactIncomingEvent.OnReceivedFirebaseUser -= HandleOnReceivedFirebaseUser;
    }
}

public class UserManagerInitializer : MonoBehaviour
{
    void Awake()
    {
        UserManager.InitailizeManually(); // You would need to create this method in UserManager
    }
}
}
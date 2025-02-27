using System.Collections;
using TMPro; // For TextMeshPro components
using UnityEngine;
using UnityEngine.UI; // For UI components
using Spaces.React.Runtime;

namespace Spaces.React.Runtime
{
    public class PopulateUIOnAuth : MonoBehaviour
    {
        public static string CurrentUserUID { get; private set; }  // Add this to store the UID

        public InputField NicknameInputField; // Assign in the inspector
        public InputField rpmURLInputField; // Assign in the inspector

        private void Awake()
        {
            Debug.Log("[PopulateUIOnAuth] Awake called - script is initializing.");
        }

        private void Start()
        {
            Debug.Log("[PopulateUIOnAuth] Start called - script is starting.");

            // Send Message To react requesting User Details.
            ReactRaiseEvent.RequestUserForUnity();
        }

        // Subscribe to event raised in react that sends user details to Unity. 
        // This event is raised in response to ReactRaiseEvent.RequestUserForUnity();
        private void OnEnable()
        {
            ReactIncomingEvent.OnReceivedFirebaseUser += HandleOnReceivedFirebaseUser;
        }

        private void OnDisable()
        {
            ReactIncomingEvent.OnReceivedFirebaseUser -= HandleOnReceivedFirebaseUser;
        }

        private void HandleOnReceivedFirebaseUser(FirebaseUserData firebaseUserData)
        {
            Debug.Log($"User: Received User from react uid: {firebaseUserData.uid}, email: {firebaseUserData.email}");
            CurrentUserUID = firebaseUserData.uid;  // Store the UID
            UpdateFieldsDirectly(firebaseUserData);
        }

        // This method is directly called from JavaScript
        public void UpdateFieldsDirectly(FirebaseUserData firebaseUserData)
        {
            Debug.Log($"[PopulateUIOnAuth] Nickname: {firebaseUserData.Nickname}, rpmURL: {firebaseUserData.rpmURL}");

            // Update UI fields with the parsed data
            UpdateFields(firebaseUserData);
        }

        private void UpdateFields(FirebaseUserData firebaseUserData)
        {
            Debug.Log($"[PopulateUIOnAuth] Updating UI fields -> Nickname: {firebaseUserData.Nickname}, rpmURL: {firebaseUserData.rpmURL}");

            if (NicknameInputField != null)
                NicknameInputField.text = firebaseUserData.Nickname;
            else
                Debug.LogWarning("[PopulateUIOnAuth] NicknameInputField is not assigned in the inspector.");

            if (rpmURLInputField != null)
                rpmURLInputField.text = firebaseUserData.rpmURL;
            else
                Debug.LogWarning("[PopulateUIOnAuth] rpmURLInputField is not assigned in the inspector.");
        }

        // Updated wrapper method to pull data from the fields
        public void UpdateFieldsDirectlyWrapper()
        {
            if (NicknameInputField != null && rpmURLInputField != null)
            {
                // Get the input from the fields
                string nickname = NicknameInputField.text;
                string rpmURL = rpmURLInputField.text;

                // Create a new FirebaseUserData object with the input values
                FirebaseUserData firebaseUserData = new FirebaseUserData
                {
                    Nickname = nickname,
                    rpmURL = rpmURL
                };

                // Call the original UpdateFieldsDirectly method with the form data
                UpdateFieldsDirectly(firebaseUserData);

                Debug.Log($"[PopulateUIOnAuth] UpdateFieldsDirectlyWrapper called with Nickname: {nickname}, rpmURL: {rpmURL}");
            }
            else
            {
                Debug.LogWarning("[PopulateUIOnAuth] NicknameInputField or rpmURLInputField is not assigned in the inspector.");
            }
        }

        [ContextMenu("Test UpdateFields")]
        public void TestUpdateFields()
        {
            Debug.Log("[PopulateUIOnAuth] Testing UpdateFields with dummy data.");
            FirebaseUserData testUser = new FirebaseUserData();
            testUser.Nickname = "Myname";
            testUser.rpmURL = "https://models.readyplayer.me/65a54ab950377ef74b6dff36.glb";
            UpdateFieldsDirectly(testUser);
        }
    }
}

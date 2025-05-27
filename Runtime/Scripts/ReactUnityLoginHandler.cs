using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Spaces.React.Runtime;

namespace Spaces.React.Runtime
{
    [Title("Spaces React Field Handler")]
    [Description("Handles UI field updates from React Firebase user data.")]

    [Category("Spaces/Login/React Field Handler")]
    public class ReactUnityFieldHandler : Instruction
    {
        [SerializeField] private PropertyGetGameObject nicknameFieldObject = new PropertyGetGameObject();
        [SerializeField] private PropertyGetGameObject rpmURLFieldObject = new PropertyGetGameObject();

        private static string currentUserUID;
        public static string CurrentUserUID => currentUserUID;

        public override string Title => "Spaces React Field Handler";

        protected override async Task Run(Args args)
        {
            Debug.Log("[Game Creator] Starting React Unity Field Handler");
            
            // Get the input fields from the GameObjects
            var nicknameField = nicknameFieldObject.Get(args)?.GetComponent<InputField>();
            var rpmURLField = rpmURLFieldObject.Get(args)?.GetComponent<InputField>();

            if (nicknameField == null || rpmURLField == null)
            {
                Debug.LogError("[Game Creator] Input fields not found. Please assign the correct GameObjects in the inspector.");
                return;
            }

            // Subscribe to the event
            ReactIncomingEvent.OnReceivedFirebaseUser += (data) => HandleOnReceivedFirebaseUser(data, nicknameField, rpmURLField);
            
            // Request user data from Firebase
            Debug.Log("[Game Creator] About to call RequestUserForUnity");
            ReactRaiseEvent.RequestUserForUnity();
            Debug.Log("[Game Creator] RequestUserForUnity called");
        }

        private void HandleOnReceivedFirebaseUser(FirebaseUserData firebaseUserData, InputField nicknameField, InputField rpmURLField)
        {
            Debug.Log($"[Game Creator] Received User from react uid: {firebaseUserData.uid}, email: {firebaseUserData.email}");
            currentUserUID = firebaseUserData.uid;  // Update the static property
            UpdateFields(firebaseUserData, nicknameField, rpmURLField);
        }

        private void UpdateFields(FirebaseUserData firebaseUserData, InputField nicknameField, InputField rpmURLField)
        {
            Debug.Log($"[Game Creator] Updating UI fields -> Nickname: {firebaseUserData.Nickname}, rpmURL: {firebaseUserData.rpmURL}");

            if (nicknameField != null)
                nicknameField.text = firebaseUserData.Nickname;
            else
                Debug.LogWarning("[Game Creator] Nickname field is not assigned.");

            if (rpmURLField != null)
                rpmURLField.text = firebaseUserData.rpmURL;
            else
                Debug.LogWarning("[Game Creator] RPM URL field is not assigned.");
        }

        [ContextMenu("Test Update Fields")]
        public void TestUpdateFields()
        {
            Debug.Log("[Game Creator] Testing UpdateFields with dummy data");
            FirebaseUserData testUser = new FirebaseUserData
            {
                Nickname = "TestUser",
                rpmURL = "https://models.readyplayer.me/682b6c39994f7261dd46350d.glb",
                uid = "test-uid",
                email = "test@example.com"
            };
            HandleOnReceivedFirebaseUser(testUser, null, null);
        }
    }
} 
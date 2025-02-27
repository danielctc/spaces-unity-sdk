using System;
using UnityEngine;
using UnityEngine.InputSystem;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;

namespace Spaces.Core.Runtime
{
    public class GCDirectionalControlToggle : MonoBehaviour
    {
        private InputPropertyValueVector2 m_InputMove;
        [SerializeField] private InputButtonKeyboardRelease keyboardRelease;

        private Character characterComponent;
        private bool isKeyboardInput = false;

        private void Start()
        {
            Debug.Log("GCDirectionalControlToggle script has started.");

            // Search for Character component in parent or an ancestor GameObjects
            characterComponent = GetComponentInParent<Character>();
            if (characterComponent == null)
            {
                Debug.LogError("No Character component found in parent hierarchy. Disabling GCDirectionalControlToggle.");
                this.enabled = false;
                return;
            }

            Debug.Log("Character component found.");

            if (characterComponent.Player == null)
            {
                Debug.LogError("Character component found but Player property is null. Disabling GCDirectionalControlToggle.");
                this.enabled = false;
                return;
            }

            Debug.Log("Player component within Character found.");

            m_InputMove = InputValueVector2KeyboardWASDArrows.Create();
            m_InputMove.OnStartup();

            if (keyboardRelease == null)
            {
                Debug.LogError("No InputButtonKeyboardRelease assigned. Movement might not stop on key release.");
            }
        }

        private void Update()
        {
            if (characterComponent.Player.IsControllable)
            {
                if (IsDirectionalInputActive())
                {
                    isKeyboardInput = true;
                    HandleDirectionalMovement();
                }
                else if (keyboardRelease != null && isKeyboardInput)
                {
                    isKeyboardInput = false;
                    StopCharacterMovement();
                }
                else if (IsPointAndClickActive())
                {
                    isKeyboardInput = false;
                    HandleDirectionalMovement();
                }
            }
            else
            {
                if (isKeyboardInput)
                {
                    isKeyboardInput = false;
                    StopCharacterMovement();
                }
            }
        }

        private void HandleDirectionalMovement()
        {
            Vector3 inputMovement = this.characterComponent.IsPlayer 
                ? m_InputMove.Read() 
                : Vector2.zero;

            Vector3 moveDirection = GetMoveDirection(inputMovement);
            float speed = characterComponent.Motion?.LinearSpeed ?? 0f;
            
            characterComponent.Motion?.MoveToDirection(moveDirection * speed, Space.World, 0);
        }

        private void StopCharacterMovement()
        {
            characterComponent.Motion?.StopToDirection();
        }

        private bool IsDirectionalInputActive()
        {
            return m_InputMove.Read() != Vector2.zero;
        }

        private bool IsPointAndClickActive()
        {
            // Placeholder for point and click logic
            return false;
        }

        protected Vector3 GetMoveDirection(Vector3 input)
        {
            Vector3 direction = new Vector3(input.x, 0f, input.y);
            Camera cameraComponent = Camera.main;

            Vector3 moveDirection = cameraComponent != null
                ? cameraComponent.transform.TransformDirection(direction)
                : Vector3.zero;

            moveDirection.y = 0;
            moveDirection.Normalize();

            return moveDirection * direction.magnitude;
        }
    }

    [Serializable]
    public class InputValueVector2KeyboardWASDArrows : TInputValueVector2
    {
        [NonSerialized] private InputAction m_InputAction;

        public InputAction InputAction
        {
            get
            {
                if (this.m_InputAction == null)
                {
                    this.m_InputAction = new InputAction("WASD/Arrows", InputActionType.Value);

                    this.m_InputAction.AddCompositeBinding("2DVector")
                        .With("Up", "<Keyboard>/w")
                        .With("Up", "<Keyboard>/upArrow")
                        .With("Down", "<Keyboard>/s")
                        .With("Down", "<Keyboard>/downArrow")
                        .With("Left", "<Keyboard>/a")
                        .With("Left", "<Keyboard>/leftArrow")
                        .With("Right", "<Keyboard>/d")
                        .With("Right", "<Keyboard>/rightArrow");
                }

                return this.m_InputAction;
            }
        }

        public static InputPropertyValueVector2 Create()
        {
            return new InputPropertyValueVector2(new InputValueVector2KeyboardWASDArrows());
        }

        public override void OnStartup()
        {
            this.Enable();
        }

        public override void OnDispose()
        {
            this.Disable();
            this.InputAction?.Dispose();
        }

        public override Vector2 Read()
        {
            return this.InputAction?.ReadValue<Vector2>() ?? Vector2.zero;
        }

        private void Enable()
        {
            this.InputAction?.Enable();
        }

        private void Disable()
        {
            this.InputAction?.Disable();
        }
    }
}

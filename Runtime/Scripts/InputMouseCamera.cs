using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GameCreator.Runtime.Common
{
    [Title("Mouse Hold")]
    [Category("Mouse/Mouse Hold")]
    
    [Description("Returns a value while a specified mouse button is held down")]
    [Image(typeof(IconMouse), ColorTheme.Type.Red)]
    
    [Serializable]
    public class InputValueVector2MouseHold : TInputValueVector2
    {
        private enum MouseButton
        {
            LeftButton,
            MiddleButton,
            RightButton
        }
        
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField] private MouseButton m_Button = MouseButton.LeftButton;
        [NonSerialized] private InputAction m_InputAction;

        // PROPERTIES: ----------------------------------------------------------------------------

        public InputAction InputAction
        {
            get
            {
                if (this.m_InputAction == null)
                {
                    this.m_InputAction = new InputAction(
                        name: "Mouse Hold", 
                        type: InputActionType.PassThrough,
                        binding: "<Mouse>/delta"
                    );
                }

                return this.m_InputAction;
            }
        }

        // INITIALIZERS: --------------------------------------------------------------------------

        public static InputPropertyValueVector2 Create()
        {
            return new InputPropertyValueVector2(
                new InputValueVector2MouseHold()
            );
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

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override Vector2 Read()
        {
            // Returns mouse delta if the specified button is held down, otherwise Vector2.zero
            if (this.m_Button == MouseButton.LeftButton && Mouse.current.leftButton.isPressed)
            {
                return this.InputAction?.ReadValue<Vector2>() ?? Vector2.zero;
            }
            else if (this.m_Button == MouseButton.MiddleButton && Mouse.current.middleButton.isPressed)
            {
                return this.InputAction?.ReadValue<Vector2>() ?? Vector2.zero;
            }
            else if (this.m_Button == MouseButton.RightButton && Mouse.current.rightButton.isPressed)
            {
                return this.InputAction?.ReadValue<Vector2>() ?? Vector2.zero;
            }

            return Vector2.zero;
        }
        
        // PRIVATE METHODS: -----------------------------------------------------------------------

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

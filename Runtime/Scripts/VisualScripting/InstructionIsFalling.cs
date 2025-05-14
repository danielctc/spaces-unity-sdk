using System;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.VisualScripting
{
    [Version(0, 2, 0)]

    [Title("On Falling Below Floor")]
    [Image(typeof(IconFall), ColorTheme.Type.Red, typeof(OverlayArrowDown))]

    [Category("Characters/Navigation/On Falling Below Floor")]
    [Description("Executes if the character falls below a certain Y position for more than a few seconds")]

    [Keywords("Fall", "Below", "Floor", "Out of Bounds", "Void")]

    [Serializable]
    public class EventCharacterOnFallingBelowFloor : TEventCharacter
    {
        [SerializeField] private float floorYThreshold = -10f;
        [SerializeField] private float fallDurationThreshold = 3f;

        private float fallTimer = 0f;
        private bool eventTriggered = false;

        // INITIALIZATION: ------------------------------------------------------------------------

        protected override void WhenEnabled(Trigger trigger, Character character)
        {
            character.EventAfterLateUpdate += this.CheckFallBelowFloor;
        }

        protected override void WhenDisabled(Trigger trigger, Character character)
        {
            character.EventAfterLateUpdate -= this.CheckFallBelowFloor;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void CheckFallBelowFloor()
        {
            Character character = this.m_Character.Get<Character>(this.m_Trigger.gameObject);
            if (character == null) return;

            float yPosition = character.transform.position.y;

            if (yPosition < floorYThreshold)
            {
                fallTimer += Time.deltaTime;

                if (fallTimer >= fallDurationThreshold && !eventTriggered)
                {
                    eventTriggered = true;
                    _ = this.m_Trigger.Execute(character.gameObject);
                }
            }
            else
            {
                // Reset if the player gets back above the floor level
                fallTimer = 0f;
                eventTriggered = false;
            }
        }
    }
}

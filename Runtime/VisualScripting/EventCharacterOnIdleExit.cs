using System;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.VisualScripting
{
    [Version(1, 0, 0)]
    [Title("On Idle Exit")]
    [Category("Characters/Navigation/On Idle Exit")]
    [Description("Executes when the specified game object exits the idle state and starts moving")]

    [Parameter("Time Mode", "The time scale in which the idle time is calculated")]
    [Parameter("Idle Time Threshold", "Amount of seconds of inactivity before entering idle state")]

    [Image(typeof(IconCharacter), ColorTheme.Type.Blue, typeof(OverlayDot))]

    [Keywords("Idle", "Exit", "Movement", "Active")]

    [Serializable]
    public class EventIdleExit : Event
    {
        // EXPOSED MEMBERS: -----------------------------------------------------------------------
        [SerializeField] private PropertyGetGameObject gameObjectA = new PropertyGetGameObject();
        [SerializeField] private TimeMode m_TimeMode = new TimeMode(TimeMode.UpdateMode.GameTime);
        [SerializeField] private PropertyGetDecimal m_IdleTimeThreshold = new PropertyGetDecimal(5f); // Default 5 seconds idle time

        // MEMBERS: -------------------------------------------------------------------------------
        private Vector3 m_LastPosition;
        private float m_IdleTimer;
        private bool m_IsIdle;

        // METHODS: -------------------------------------------------------------------------------

        protected override void OnEnable(Trigger trigger)
        {
            base.OnEnable(trigger);
            ResetIdleState(trigger);
        }

        protected override void OnDisable(Trigger trigger)
        {
            base.OnDisable(trigger);
            m_IdleTimer = 0f;
            m_IsIdle = false;
        }

        protected override void OnUpdate(Trigger trigger)
        {
            base.OnUpdate(trigger);

            GameObject target = this.gameObjectA.Get(trigger.gameObject);
            if (target == null) return;

            // Check if the object has moved (using distance threshold for WebGL compatibility)
            float distanceMoved = Vector3.Distance(target.transform.position, m_LastPosition);
            if (distanceMoved > 0.001f) // Small threshold to account for floating-point precision in WebGL
            {
                if (m_IsIdle)
                {
                    OnIdleExitTriggered(trigger);
                }

                ResetIdleState(trigger);
                m_LastPosition = target.transform.position;
                return;
            }

            // Increase the idle timer
            m_IdleTimer += Time.deltaTime;

            if (!m_IsIdle && m_IdleTimer >= (float)this.m_IdleTimeThreshold.Get(trigger.gameObject))
            {
                m_IsIdle = true;
            }
        }

        private void ResetIdleState(Trigger trigger)
        {
            GameObject target = this.gameObjectA.Get(trigger.gameObject);
            if (target == null) return;

            m_LastPosition = target.transform.position;
            m_IdleTimer = 0f;
            m_IsIdle = false;
        }

        private void OnIdleExitTriggered(Trigger trigger)
        {
            GameObject target = this.gameObjectA.Get(trigger.gameObject);
            if (target == null) return;

            // Force animation state update for WebGL/Fusion compatibility
            var animator = target.GetComponentInChildren<Animator>();
            if (animator != null)
            {
                // Ensure the animator is properly updated
                animator.Update(0);
            }

            // Event logic when the target exits idle state
            _ = trigger.Execute(this.Self);

            // Reset the idle state
            ResetIdleState(trigger);
        }
    }
}

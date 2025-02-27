using System;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.VisualScripting
{
    [Title("Compare Car Damage")]
    [Description("Returns true if the comparison between a number and the Character's speed is satisfied")]

    [Category("Car/Compare Car Damage")]

    [Keywords("Damage", "Car", "Current")]

    [Image(typeof(IconPhysics), ColorTheme.Type.Blue)]
    [Serializable]
    public class ConditionCurrentDamage : Condition
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField] private PropertyGetGameObject m_GameObject = new PropertyGetGameObject();
        [SerializeField] public float damageThreshold = 50f;

        // PROPERTIES: ----------------------------------------------------------------------------

        protected override string Summary => $"Damage of {this.m_GameObject} {this.damageThreshold}";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override bool Run(Args args)
        {
            GameObject gameObject = this.m_GameObject.Get(args);
            GC2CarController carController = gameObject.GetComponent<GC2CarController>();
            return carController != null && carController.totalDamage >= this.damageThreshold;
        }
    }
}

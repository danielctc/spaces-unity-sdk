using System;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.VisualScripting
{
    [Title("Check Front Damage")]
    [Description("Returns true if the damage on the car is done on the front")]

    [Category("Car/Check Front Damage")]

    [Keywords("Damage", "Car", "Front")]

    [Image(typeof(IconPhysics), ColorTheme.Type.Blue)]
    [Serializable]
    public class ConditionFrontDamage : Condition
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField] private PropertyGetGameObject m_GameObject = new PropertyGetGameObject();

        // PROPERTIES: ----------------------------------------------------------------------------

        protected override string Summary => $"Damage on the front of this {this.m_GameObject}";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override bool Run(Args args)
        {
            GameObject gameObject = this.m_GameObject.Get(args);
            GC2CarController carController = gameObject.GetComponent<GC2CarController>();
            return carController != null && carController.damageFront;
        }
    }
}

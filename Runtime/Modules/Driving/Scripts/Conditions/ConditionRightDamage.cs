using System;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.VisualScripting
{
    [Title("Check Right Damage")]
    [Description("Returns true if the damage on the car is done on the right")]

    [Category("Car/Check Right Damage")]

    [Keywords("Damage", "Car", "Right")]

    [Image(typeof(IconPhysics), ColorTheme.Type.Blue)]
    [Serializable]
    public class ConditionRightDamage : Condition
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField] private PropertyGetGameObject m_GameObject = new PropertyGetGameObject();

        // PROPERTIES: ----------------------------------------------------------------------------

        protected override string Summary => $"Damage on the right of this {this.m_GameObject}";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override bool Run(Args args)
        {
            GameObject gameObject = this.m_GameObject.Get(args);
            GC2CarController carController = gameObject.GetComponent<GC2CarController>();
            return carController != null && carController.damageRight;
        }
    }
}

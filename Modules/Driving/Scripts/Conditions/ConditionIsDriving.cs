using System;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.VisualScripting
{
    [Title("Is Car Driving")]
    [Description("Returns true if the GC2CarController component's isDriving field is true")]

    [Category("Car/Is Car Driving")]

    [Parameter("Game Object", "The game object that has the GC2CarController component")]
    [Keywords("Car", "Driving", "Active")]

    [Image(typeof(IconComponent), ColorTheme.Type.Blue)]
    [Serializable]
    public class ConditionIsDriving : Condition
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [SerializeField] private PropertyGetGameObject m_GameObject = new PropertyGetGameObject();

        // PROPERTIES: ----------------------------------------------------------------------------

        protected override string Summary => $"is {this.m_GameObject} active?";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override bool Run(Args args)
        {
            GameObject gameObject = this.m_GameObject.Get(args);
            if (gameObject != null && gameObject.activeInHierarchy)
            {
                GC2CarController carController = gameObject.GetComponent<GC2CarController>();
                if (carController != null)
                {
                    return carController.isDriving;
                }
            }
            return false;
        }
    }

}
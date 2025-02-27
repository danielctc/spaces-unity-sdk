using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.VisualScripting
{
    [Title("Enable Car")]
    [Description("Enables or disables the Car Controller")]

    [Category("Car/Enable Car")]

    [Keywords("Car", "Enable", "Disable")]

    [Image(typeof(IconWheel), ColorTheme.Type.Blue)]

    [Serializable]
    public class InstructionEnableCar : Instruction
    {
        [SerializeField] private PropertyGetGameObject m_GameObject = new PropertyGetGameObject();
        [SerializeField] private bool controlCar = new bool();

        // PROPERTIES: ----------------------------------------------------------------------------

        public override string Title => $"Enable or disable {this.m_GameObject}";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override async Task Run(Args args)
        {
            GameObject gameObject = this.m_GameObject.Get(args);
            if (gameObject == null) return;
            {
                GC2CarController carController = gameObject.GetComponent<GC2CarController>();
                if (carController != null)
                {
                    carController.isDriving = controlCar;
                }
            }
        }
    }
}
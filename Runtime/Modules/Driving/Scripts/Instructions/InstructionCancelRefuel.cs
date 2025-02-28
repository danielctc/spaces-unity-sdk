using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.VisualScripting
{
    [Title("Cancel Refuel Car")]
    [Description("Cancels the refuel instruction")]

    [Category("Car/Cancel Refuel")]

    [Keywords("Car", "Refuel", "Fuel")]

    [Image(typeof(IconTriggers), ColorTheme.Type.Blue)]

    [Serializable]
    public class InstructionCancelRefuel : Instruction
    {
        [SerializeField] private PropertyGetGameObject m_GameObject = new PropertyGetGameObject();

        // PROPERTIES: ----------------------------------------------------------------------------

        public override string Title => $"Cancel refuel on this {this.m_GameObject}";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override async Task Run(Args args)
        {
            GameObject gameObject = this.m_GameObject.Get(args);
            if (gameObject == null) return;
            {
                GC2CarController carController = gameObject.GetComponent<GC2CarController>();
                if (carController != null)
                {
                    carController.CancelRefuel();
                    await Task.Delay(TimeSpan.FromSeconds(100f));
                    carController.ResetCancelRefuel();
                }
            }
        }
    }
}
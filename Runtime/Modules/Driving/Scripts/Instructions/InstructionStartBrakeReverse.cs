using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.VisualScripting
{
    [Title("Brake & Reverse Start")]
    [Description("Brake & reverse the Car Controller")]

    [Category("Car/Brake & Reverse Start")]

    [Keywords("Car", "Brake","Reverse", "Start")]

    [Image(typeof(IconWheel), ColorTheme.Type.Blue)]

    [Serializable]
    public class InstructionStartBrakeReverse : Instruction
    {
        [SerializeField] private PropertyGetGameObject m_GameObject = new PropertyGetGameObject();

        // PROPERTIES: ----------------------------------------------------------------------------

        public override string Title => $"Start brake & reverse on this {this.m_GameObject}";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override async Task Run(Args args)
        {
            GameObject gameObject = this.m_GameObject.Get(args);
            if (gameObject == null) return;
            {
                GC2CarController carController = gameObject.GetComponent<GC2CarController>();
                if (carController != null)
                {
                    carController.StartBraking();
                }
            }
        }
    }
}
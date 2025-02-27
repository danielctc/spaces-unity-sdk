using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.VisualScripting
{
    [Title("Look Left Start")]
    [Description("Look Left with the Car Camera")]

    [Category("Car/Look Left Start")]

    [Keywords("Car", "Look", "Start")]

    [Image(typeof(IconCamera), ColorTheme.Type.Blue)]

    [Serializable]
    public class InstructionStarLookLeft : Instruction
    {
        [SerializeField] private PropertyGetGameObject m_GameObject = new PropertyGetGameObject();

        // PROPERTIES: ----------------------------------------------------------------------------

        public override string Title => $"Start look left for this {this.m_GameObject}";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override async Task Run(Args args)
        {
            GameObject gameObject = this.m_GameObject.Get(args);
            if (gameObject == null) return;
            {
                GC2CarCamera carCamera = gameObject.GetComponent<GC2CarCamera>();
                if (carCamera != null)
                {
                    carCamera.StartLookLeft();
                }
            }
        }
    }
}
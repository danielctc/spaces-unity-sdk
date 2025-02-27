using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.VisualScripting
{
    [Title("Repair Car")]
    [Description("Repairs a car with a specified value")]

    [Category("Car/Repair Car")]

    [Keywords("Car", "Repair", "Damage")]

    [Image(typeof(IconPhysics), ColorTheme.Type.Blue)]

    [Serializable]
    public class InstructionRepair : Instruction
    {
        [SerializeField] private PropertyGetGameObject m_GameObject = new PropertyGetGameObject();
        [SerializeField] private PropertyGetDecimal repairAmount = new PropertyGetDecimal(10f);
        [SerializeField] private Transition m_Transition = new Transition();

        private GC2CarController carController;

        // PROPERTIES: ----------------------------------------------------------------------------

        public override string Title => $"Repair {this.m_GameObject} to {this.repairAmount}";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override async Task Run(Args args)
        {
            GameObject gameObject = this.m_GameObject.Get(args);
            if (gameObject == null) return;

            carController = gameObject.GetComponent<GC2CarController>();

            if (carController != null)
            {
                float startDamage = carController.totalDamage;
                float endDamage = Mathf.Max(0f, startDamage - (float)repairAmount.Get(args));

                float valueSource = startDamage;
                float valueTarget = endDamage;

                ITweenInput tween = new TweenInput<float>(
                    valueSource,
                    valueTarget,
                    this.m_Transition.Duration,
                    (a, b, t) => carController.totalDamage = Mathf.Lerp(startDamage, endDamage, t),
                    Tween.GetHash(typeof(GC2CarController), "totalDamage"),
                    this.m_Transition.EasingType,
                    this.m_Transition.Time
                );

                Tween.To(gameObject, tween);
                if (this.m_Transition.WaitToComplete) await this.Until(() => tween.IsFinished);

                carController.totalDamage = endDamage;
            }
        }
    }
}

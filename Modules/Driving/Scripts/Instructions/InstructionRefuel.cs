using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.VisualScripting
{
    [Title("Refuel Car")]
    [Description("Refuels a car with a specified amount of fuel")]

    [Category("Car/Refuel Car")]

    [Keywords("Car", "Refuel", "Fuel")]

    [Image(typeof(IconTriggers), ColorTheme.Type.Blue)]

    [Serializable]
    public class InstructionRefuel : Instruction
    {
        [SerializeField] private PropertyGetGameObject m_GameObject = new PropertyGetGameObject();
        [SerializeField] private PropertyGetDecimal fuelAmount = new PropertyGetDecimal(10f);
        [SerializeField] private Transition m_Transition = new Transition();

        private GC2CarController carController; 

        // PROPERTIES: ----------------------------------------------------------------------------

        public override string Title => $"Fuel {this.m_GameObject} to {this.fuelAmount}";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override async Task Run(Args args)
        {
            GameObject gameObject = this.m_GameObject.Get(args);
            if (gameObject == null) return;

            carController = gameObject.GetComponent<GC2CarController>(); 

            if (carController != null)
            {
                float maxFuel = (float)carController.maxFuelAmount.Get(args);
                float startFuel = carController.currentFuelAmount;
                float fuelToAdd = (float)fuelAmount.Get(args);
                float endFuel = Mathf.Min(startFuel + fuelToAdd, maxFuel);
                float remainingFuelSpace = maxFuel - startFuel;
                float fuelToAddCapped = Mathf.Min(fuelToAdd, remainingFuelSpace);

                float valueSource = startFuel;
                float valueTarget = endFuel;

                ITweenInput tween = new TweenInput<float>(
                    valueSource,
                    valueTarget,
                    this.m_Transition.Duration,
                    (a, b, t) =>
                    {
                        if (carController.cancelRefuel)
                        {
                            Tween.Cancel(
                                gameObject,
                                Tween.GetHash(typeof(GC2CarController), "currentFuelAmount"));
                            return;
                        }
                        carController.currentFuelAmount = Mathf.Lerp(a, b, t);


                    },
                    Tween.GetHash(typeof(GC2CarController), "currentFuelAmount"),
                    this.m_Transition.EasingType,
                    this.m_Transition.Time
                );

                Tween.To(gameObject, tween);
                if (this.m_Transition.WaitToComplete) await this.Until(() => tween.IsFinished);

                if (carController.cancelRefuel)
                {
                    tween.OnCancel();
                    return;
                }
            }
        }

        public void CancelRefuel(Args args)
        {
            GameObject gameObject = this.m_GameObject.Get(args);
            if (gameObject == null) return;

            GC2CarController carController = gameObject.GetComponent<GC2CarController>();
            if (carController != null)
            {
                carController.cancelRefuel = true;
            }
        }

    }
}

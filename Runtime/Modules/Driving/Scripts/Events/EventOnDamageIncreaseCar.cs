using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Variables;
using UnityEngine;

namespace GameCreator.Runtime.VisualScripting
{
    [Title("On Damage Increase")]
    [Category("Car/On Damage Increase")]
    [Description("Executed every time the car damage increases")]

    [Image(typeof(IconLoop), ColorTheme.Type.Blue)]

    [Keywords("Car", "Damage")]
    [Serializable]
    public class EventOnDamageIncreaseCar : GameCreator.Runtime.VisualScripting.Event
    {
        public GC2CarController carController;
        [HideInInspector] public float prevDamage;

        protected override void OnUpdate(Trigger trigger)
        {
            base.OnUpdate(trigger);

            if (this.carController == null)
            {
                this.carController = trigger.gameObject.GetComponent<GC2CarController>();
                if (this.carController == null)
                {
                    return;
                }
                this.prevDamage = this.carController.totalDamage;
            }

            if (this.carController.totalDamage > this.prevDamage)
            {
                _ = trigger.Execute(this.Self);
            }
            this.prevDamage = this.carController.totalDamage;
        }
    }
}
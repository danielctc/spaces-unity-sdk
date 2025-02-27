using UnityEngine;
using UnityEngine.UI;

public class GC2CarDisplayDamage : MonoBehaviour
{
    public GC2CarController carController;
    public Text damageText;

    private void Update()
    {
        float damage = carController.totalDamage;
        damageText.text = damage.ToString("0");
    }
}

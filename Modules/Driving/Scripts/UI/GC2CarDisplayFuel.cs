using UnityEngine;
using UnityEngine.UI;

public class GC2CarDisplayFuel : MonoBehaviour
{
    public GC2CarController carController;
    public Text fuelText;
    public Image fuelImage;

    private void Update()
    {
        float currentFuelAmount = carController.currentFuelAmount;
        float maxFuelAmount = (float) carController.maxFuelAmount.Get(this.gameObject);

        if (fuelText != null)
        {
            fuelText.text = string.Format("{0:N1}/{1:N1} L", currentFuelAmount, maxFuelAmount);
        }

        if (fuelImage != null)
        {
            float fuelRatio = currentFuelAmount / maxFuelAmount;
            fuelImage.fillAmount = fuelRatio;
        }
    }
}

using UnityEngine;
using UnityEngine.UI;

public class GC2CarDisplaySpeed : MonoBehaviour
{
    public GC2CarController carController;
    public Text speedText;
    public Image speedometer;

    private void Update()
    {
        float speed = carController.currentSpeed;
        speedText.text = speed.ToString("0") + " km/h";
        float rotation = Mathf.Clamp(speed / (float) carController.maxSpeed.Get(this.gameObject), 0f, 1f) * 180f;
        speedometer.transform.rotation = Quaternion.Euler(0f, 0f, -rotation);
    }
}

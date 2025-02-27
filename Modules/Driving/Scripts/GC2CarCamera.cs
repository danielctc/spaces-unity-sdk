using GameCreator.Runtime.Common;
using UnityEngine;

public class GC2CarCamera : MonoBehaviour
{
    public Transform carTransform;
    [Space]
    [SerializeField] private PropertyGetDecimal speedThreshold = new PropertyGetDecimal(5f);
    [SerializeField] private PropertyGetDecimal minFOV = new PropertyGetDecimal(60f);
    [SerializeField] private PropertyGetDecimal maxFOV = new PropertyGetDecimal(70f);
    [Space]
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float maxRotationAngle = 45f;
    [SerializeField] private float rotationSmoothing = 10f;
    [Space]
    private bool lookLeft;
    private bool lookRight;

    private Camera camera;
    private float currentFOV;
    private float currentRotation = 0f;
    private float targetRotation = 0f;

    private void Start()
    {
        camera = GetComponent<Camera>();
        currentFOV = camera.fieldOfView;
    }

    public void StartLookLeft()
    {
        lookLeft = true;
    }

    public void StopLookLeft()
    {
        lookLeft = false;
    }

    public void StartLookRight()
    {
        lookRight = true;
    }

    public void StopLookRight()
    {
        lookRight = false;
    }

    private void Update()
    {
        float speed = carTransform.GetComponent<GC2CarController>().currentSpeed;
        bool isBreaking = carTransform.GetComponent<GC2CarController>().isBreaking;

        float targetFOV = speed >= (float)this.speedThreshold.Get(this.gameObject) ? (float)this.maxFOV.Get(this.gameObject) : (float)this.minFOV.Get(this.gameObject);

        if (isBreaking)
        {
            targetFOV = Mathf.Clamp(targetFOV - 10f, (float)this.minFOV.Get(this.gameObject), (float) this.maxFOV.Get(this.gameObject));
        }

        currentFOV = Mathf.Lerp(currentFOV, targetFOV, Time.deltaTime);
        camera.fieldOfView = currentFOV;
        camera.nearClipPlane = 0.01f;

        float rotationInput = 0f;
        if (lookLeft)
        {
            rotationInput = -1f;
        }
        else if (lookRight)
        {
            rotationInput = 1f;
        }
        else
        {
            targetRotation = 0f;
        }

        float newTargetRotation = Mathf.Clamp(currentRotation + rotationInput * rotationSpeed, -maxRotationAngle, maxRotationAngle);

        if (Mathf.Abs(newTargetRotation - currentRotation) > 0.1f)
        {
            targetRotation = newTargetRotation;
        }

        currentRotation = Mathf.Lerp(currentRotation, targetRotation, Time.deltaTime * rotationSmoothing);

        transform.localEulerAngles = new Vector3(0f, currentRotation, 0f);
    }
}

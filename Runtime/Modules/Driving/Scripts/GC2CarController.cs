using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("")]
public class GC2CarController : MonoBehaviour
{
    private const string HORIZONTAL = "Horizontal";
    private const string VERTICAL = "Vertical";

    private float horizontalInput;
    private float verticalInput;
    private float currentSteerAngle;
    private float currentBreakForce;
    private bool isFlippedOver = false;
    public bool isBreaking;

    private AudioSource audioSource;
    [SerializeField] private AudioSource breakingAudioSource;
    private Rigidbody carRigidbody;

    [SerializeField] public bool isDriving = true;
    [SerializeField] private PropertyGetDecimal carMass = new PropertyGetDecimal(1500f);
    [SerializeField] public Vector3 centerOfGravity = new Vector3(0f, -0.2f, 0f);

    public enum DriveMode { TwoWheelDrive, FourWheelDrive };
    [SerializeField] private DriveMode driveMode = DriveMode.FourWheelDrive;
    [SerializeField] private PropertyGetDecimal motorForce = new PropertyGetDecimal(1000f);
    [SerializeField] private PropertyGetDecimal breakForce = new PropertyGetDecimal(3000.0f);
    [SerializeField] private float hardBreakForce = 6000.0f;
    [SerializeField] private PropertyGetDecimal steerSpeed = new PropertyGetDecimal(5f);
    [SerializeField] private Transform steeringWheel;
    [SerializeField] public Text speedText;
    [SerializeField] public Image speedometer;
    [SerializeField] private Transform speedometerPivot;
    public enum RotationAxis { X, Y, Z };
    [SerializeField] private RotationAxis rotationAxis = RotationAxis.Y;
    [SerializeField] private PropertyGetDecimal maxSteerAngle = new PropertyGetDecimal(30.0f);
    [SerializeField] public PropertyGetDecimal maxSpeed = new PropertyGetDecimal(120.0f);

    [SerializeField] public PropertyGetDecimal maxFuelAmount = new PropertyGetDecimal(50f);
    [SerializeField] private PropertyGetDecimal fuelConsumptionRate = new PropertyGetDecimal(1f);
    [HideInInspector] public float currentFuelAmount;
    [SerializeField] public Text fuelText;
    [SerializeField] public Image fuelImage;
    [SerializeField] private RectTransform fuelImageRect;
    [SerializeField] public bool cancelRefuel = false;
    [SerializeField] public static bool cancelAllRefuels = false;

    [SerializeField] private PropertyGetDecimal wheelDampingRateValue = new PropertyGetDecimal(0.25f);
    [SerializeField] private PropertyGetDecimal suspensionDistanceValue = new PropertyGetDecimal(0.3f);
    [SerializeField] private PropertyGetDecimal suspensionTargetValue = new PropertyGetDecimal(0.5f);
    [SerializeField] private PropertyGetDecimal suspensionSpringValue = new PropertyGetDecimal(35000f);
    [SerializeField] private PropertyGetDecimal suspensionDamperValue = new PropertyGetDecimal(4500f);

    [Space]
    [SerializeField] private WheelCollider FL_Collider;
    [SerializeField] private WheelCollider FR_Collider;
    [SerializeField] private WheelCollider RL_Collider;
    [SerializeField] private WheelCollider RR_Collider;

    [Space]
    [SerializeField] private Transform FL_Joint;
    [SerializeField] private Transform FR_Joint;
    [SerializeField] private Transform RL_Joint;
    [SerializeField] private Transform RR_Joint;

    [SerializeField] public Text damageText;
    [SerializeField] private PropertyGetDecimal maxCollisionDamage = new PropertyGetDecimal(100f);
    [SerializeField] private float damageMultiplier = 1.0f;
    [SerializeField] private float massDamageReductionFactor = 0.1f;
    [HideInInspector] public float currentSpeed;
    [HideInInspector] public float collisionDamageFactor = 0.1f;
    [HideInInspector] public float currentCollisionDamage = 0f;
    [HideInInspector] public bool damageFront = false;
    [HideInInspector] public bool damageRear = false;
    [HideInInspector] public bool damageLeft = false;
    [HideInInspector] public bool damageRight = false;
    [SerializeField] public float m_TotalDamage = 0f;

    public float totalDamage
    {
        get => this.m_TotalDamage;
        set
        {
            this.m_TotalDamage = value;
            this.UpdateDamageText();
        }
    }

    private bool accelerating;
    private bool braking;

    [SerializeField] private AudioClip startAudioClip;
    [SerializeField] private AudioClip engineAudioClip;
    [SerializeField] private AudioClip breakingAudioClip;
    [SerializeField] public float minVolume = 0.6f;
    [SerializeField] public float maxVolume = 1.0f;
    [SerializeField] public float minPitch = 0.5f;
    [SerializeField] public float maxPitch = 1.5f;

    [Space]
    [SerializeField] public InstructionList onIdle = new InstructionList();
    [SerializeField] public InstructionList onAcceleration = new InstructionList();
    [SerializeField] public InstructionList onBrakeReverse = new InstructionList();

    private void Start()
    {
        carRigidbody = GetComponent<Rigidbody>();
        if (carRigidbody == null)
        {
            carRigidbody = gameObject.AddComponent<Rigidbody>();
        }

        carRigidbody.mass = (float)this.carMass.Get(this.gameObject);
        currentFuelAmount = (float)this.maxFuelAmount.Get(this.gameObject);
//        fuelImageRect = fuelImage.GetComponent<RectTransform>();
        audioSource = GetComponent<AudioSource>();
        breakingAudioSource.clip = breakingAudioClip;

        if (damageText != null)
        {
            damageText.text = "0";
        }

        AdjustCenterOfGravity();
    }

    private void AdjustCenterOfGravity()
    {
        if (carRigidbody != null)
        {
            carRigidbody.centerOfMass = centerOfGravity;
        }
    }

    public void StartAcceleration()
    {
        accelerating = true;
    }

    public void StopAcceleration()
    {
        accelerating = false;
    }

    public void StartBraking()
    {
        braking = true;
    }

    public void StopBraking()
    {
        braking = false;
    }

public void StopAllAudio()
{
    StartCoroutine(FadeOutAudio(audioSource, 1.0f)); // Fades out over 1 second
    StartCoroutine(FadeOutAudio(breakingAudioSource, 1.0f)); // Fades out over 1 second
}

private IEnumerator FadeOutAudio(AudioSource audioSrc, float fadeTime)
{
    if (audioSrc == null || !audioSrc.isPlaying) yield break;

    float startVolume = audioSrc.volume;

    while (audioSrc.volume > 0)
    {
        audioSrc.volume -= startVolume * Time.deltaTime / fadeTime;
        yield return null;
    }

    audioSrc.Stop();
    audioSrc.volume = startVolume;
}


    private void FixedUpdate()
    {
        if (isDriving)
        {
            GetInput();
            HandleMotor();
            HandleSteering();
            UpdateWheels();
            UpdateSpeed();
            CheckIfFlippedOver();

            if (isBreaking)
            {
                if (!breakingAudioSource.isPlaying)
                {
                    breakingAudioSource.Play();
                }
            }
            else
            {
                breakingAudioSource.Stop();
                audioSource.clip = engineAudioClip;
                audioSource.loop = true;
                if (!audioSource.isPlaying && audioSource.clip != startAudioClip)
                {
                    audioSource.Play();
                }
            }
        }
    }

    private void GetInput()
    {
        horizontalInput = Input.GetAxis(HORIZONTAL);
        isBreaking = false;
        float forwardVelocity = Vector3.Dot(carRigidbody.linearVelocity, transform.forward);
        float backwardVelocity = Vector3.Dot(carRigidbody.linearVelocity, -transform.forward);

        if (Mathf.Approximately(forwardVelocity, 0f) && Mathf.Approximately(backwardVelocity, 0f))
        {
            if (braking)
            {
                isBreaking = true;
                verticalInput = -1f;
            }
            else if (accelerating)
            {
                verticalInput = 1f;
            }
            else
            {
                verticalInput = 0f;
            }
        }
        else if (forwardVelocity > 0f)
        {
            if (braking)
            {
                isBreaking = true;
                verticalInput = -1f;
            }
            else if (accelerating)
            {
                verticalInput = 1f;
            }
            else
            {
                verticalInput = 0f;
            }
        }
        else if (backwardVelocity > 0f)
        {
            if (accelerating)
            {
                isBreaking = true;
                verticalInput = 1f;
            }
            else if (braking)
            {
                verticalInput = -1f;
            }
            else
            {
                verticalInput = 0f;
            }
        }

        if (Mathf.Approximately(forwardVelocity, 0f) && Mathf.Approximately(verticalInput, 1f))
        {
            isBreaking = false;
        }

        if (Mathf.Approximately(backwardVelocity, 0f) && Mathf.Approximately(verticalInput, -1f))
        {
            isBreaking = false;
        }
    }

    private void HandleMotor()
    {
        if (currentFuelAmount > 0f)
        {
            float currentMotorTorque = verticalInput * (float)motorForce.Get(this.gameObject);
            currentBreakForce = isBreaking ? hardBreakForce : (float)breakForce.Get(this.gameObject);

            if (Mathf.Abs(carRigidbody.linearVelocity.magnitude) < 0.1f && Mathf.Abs(verticalInput) > 0.1f)
            {
                isBreaking = false;
            }

            if (driveMode == DriveMode.TwoWheelDrive)
            {
                FL_Collider.motorTorque = currentMotorTorque;
                FR_Collider.motorTorque = currentMotorTorque;
            }
            else
            {
                FL_Collider.motorTorque = currentMotorTorque;
                FR_Collider.motorTorque = currentMotorTorque;
                RL_Collider.motorTorque = currentMotorTorque;
                RR_Collider.motorTorque = currentMotorTorque;
            }

            if (verticalInput != 0f)
            {
                currentFuelAmount -= (float)fuelConsumptionRate.Get(this.gameObject) * Time.deltaTime;
            }

            if (isBreaking)
            {
                currentBreakForce += (float)breakForce.Get(this.gameObject);
                ApplyBreaking();
            }
            else if (verticalInput == 0f)
            {
                ApplyBreaking();
            }
            else
            {
                ReleaseBreaking();
            }
        }
        else
        {
            FL_Collider.motorTorque = 0f;
            FR_Collider.motorTorque = 0f;
            RL_Collider.motorTorque = 0f;
            RR_Collider.motorTorque = 0f;
            ApplyBreaking();
        }
    }


    private void ApplyBreaking()
    {
        FR_Collider.brakeTorque = currentBreakForce;
        FL_Collider.brakeTorque = currentBreakForce;
        RL_Collider.brakeTorque = currentBreakForce;
        RR_Collider.brakeTorque = currentBreakForce;
    }

    private void ReleaseBreaking()
    {
        FL_Collider.brakeTorque = 0f;
        FR_Collider.brakeTorque = 0f;
        RL_Collider.brakeTorque = 0f;
        RR_Collider.brakeTorque = 0f;
    }

    public void AddFuel(float amount)
    {
        if (cancelRefuel) return;
        currentFuelAmount += amount;
        currentFuelAmount = Mathf.Clamp(currentFuelAmount, 0f, (float)maxFuelAmount.Get(this.gameObject));
        UpdateFuelText();
    }

    private void UpdateFuelText()
    {
        fuelText.text = "Fuel: " + currentFuelAmount.ToString("F1");
    }

    public void CancelRefuel()
    {
        this.cancelRefuel = true;
        GC2CarController.cancelAllRefuels = true;
    }

    public void ResetCancelRefuel()
    {
        this.cancelRefuel = false;
        GC2CarController.cancelAllRefuels = false;
    }

    private void HandleSteering()
    {
        float targetSteerAngle = (float)maxSteerAngle.Get(this.gameObject) * horizontalInput;
        currentSteerAngle = Mathf.Lerp(currentSteerAngle, targetSteerAngle, Time.deltaTime * (float)steerSpeed.Get(this.gameObject));

        FL_Collider.steerAngle = currentSteerAngle;
        FR_Collider.steerAngle = currentSteerAngle;

        steeringWheel.localRotation = Quaternion.Euler(
            rotationAxis == RotationAxis.X ? -currentSteerAngle : 0f,
            rotationAxis == RotationAxis.Y ? -currentSteerAngle : 0f,
            rotationAxis == RotationAxis.Z ? -currentSteerAngle : 0f
        );
    }

    private void UpdateWheels()
    {
        UpdateSingleWheel(FL_Collider, FL_Joint);
        UpdateSingleWheel(FR_Collider, FR_Joint);
        UpdateSingleWheel(RL_Collider, RL_Joint);
        UpdateSingleWheel(RR_Collider, RR_Joint);
    }

    private void UpdateSingleWheel(WheelCollider wheelCollider, Transform wheelTransform)
    {
        Vector3 pos;
        Quaternion rot;
        wheelCollider.GetWorldPose(out pos, out rot);
        wheelTransform.rotation = rot;
        wheelTransform.position = pos;

        // Set suspension values
        WheelHit hit;
        if (wheelCollider.GetGroundHit(out hit))
        {
            JointSpring suspensionSpring = wheelCollider.suspensionSpring;
            suspensionSpring.spring = (float)this.suspensionSpringValue.Get(this.gameObject);
            suspensionSpring.damper = (float)this.suspensionDamperValue.Get(this.gameObject);
            suspensionSpring.targetPosition = (float)this.suspensionTargetValue.Get(this.gameObject);
            wheelCollider.suspensionSpring = suspensionSpring;

            wheelCollider.wheelDampingRate = (float)this.wheelDampingRateValue.Get(this.gameObject);
            wheelCollider.suspensionDistance = (float)this.suspensionDistanceValue.Get(this.gameObject);
        }
    }
    public void RepairDamage(float repairAmount)
    {
        totalDamage = Mathf.Max(totalDamage - repairAmount, 0f);
        UpdateDamageText();
    }

   private void OnCollisionEnter(Collision collision)
{
    float collisionSpeed = collision.relativeVelocity.magnitude;
    float collisionDamage = collisionSpeed * damageMultiplier;
    totalDamage += collisionDamage;

    if (carRigidbody == null)
    {
        carRigidbody = GetComponent<Rigidbody>();
        if (carRigidbody == null)
        {
            return;
        }
    }

    Vector3 direction = collision.contacts[0].point - transform.position;
    float angle = Vector3.Angle(direction, transform.forward);

    if (angle < 90f)
    {
        damageFront = true;
        StartCoroutine(ResetDamageAfterDelay(0.5f, () => damageFront = false));
    }
    else if (angle > 90f && angle < 180f)
    {
        damageRear = true;
        StartCoroutine(ResetDamageAfterDelay(0.5f, () => damageRear = false));
    }
    else if (angle < 0f && angle > -90f)
    {
        damageRight = true;
        StartCoroutine(ResetDamageAfterDelay(0.5f, () => damageRight = false));
    }
    else
    {
        damageLeft = true;
        StartCoroutine(ResetDamageAfterDelay(0.5f, () => damageLeft = false));
    }

    currentCollisionDamage = collisionDamage * (1f - carRigidbody.mass * massDamageReductionFactor);
    collisionDamageFactor = currentCollisionDamage / (float)this.maxCollisionDamage.Get(this.gameObject);
    collisionDamageFactor = Mathf.Clamp01(collisionDamageFactor);

    UpdateDamageText();
}

public void UpdateDamageText()
{
    if (damageText != null)
    {
        damageText.text = "" + Mathf.RoundToInt(totalDamage);
    }
}


    private void CheckIfFlippedOver()
    {
        if (transform.up.y < 0f && !isFlippedOver)
        {
            isFlippedOver = true;
            StartCoroutine(ResetCar());
        }
    }

    private IEnumerator ResetCar()
    {
        yield return new WaitForSeconds(2f);

        transform.position = new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z);
        transform.rotation = Quaternion.identity;

        carRigidbody.linearVelocity = Vector3.zero;
        carRigidbody.angularVelocity = Vector3.zero;

        yield return new WaitForSeconds(1f);

        isFlippedOver = false;
    }

    private IEnumerator ResetDamageAfterDelay(float delay, Action action)
    {
        yield return new WaitForSeconds(delay);
        action?.Invoke();
    }

    private void Update()
    {
        if (fuelText != null)
        {
            fuelText.text = string.Format("{0:N1}/{1:N1} L", currentFuelAmount, maxFuelAmount.Get(this.gameObject));
        }
        if (fuelImage != null)
        {
            float fuelRatio = currentFuelAmount / (float)maxFuelAmount.Get(this.gameObject);
            fuelImage.fillAmount = fuelRatio;
        }

        if (!accelerating && !braking)
        {
            _ = this.onIdle.Run(new Args(this.gameObject));
        }

        if (accelerating)
        {
            _ = this.onAcceleration.Run(new Args(this.gameObject));
        }

        if (braking)
        {
            _ = this.onBrakeReverse.Run(new Args(this.gameObject));
        }
    }

    private void UpdateSpeed()
    {
        currentSpeed = GetComponent<Rigidbody>().linearVelocity.magnitude * 3.6f;
        if (currentSpeed > maxSpeed.Get(this.gameObject))
        {
            Vector3 normalizedVelocity = GetComponent<Rigidbody>().linearVelocity.normalized;
            GetComponent<Rigidbody>().linearVelocity = normalizedVelocity * ((float)maxSpeed.Get(this.gameObject) / 3.6f);
            currentSpeed = (float)maxSpeed.Get(this.gameObject);
        }
        speedText.text = Mathf.Round(currentSpeed) + " km/h";
        audioSource.pitch = Mathf.Lerp(minPitch, maxPitch, currentSpeed / (float)maxSpeed.Get(this.gameObject));
        breakingAudioSource.pitch = Mathf.Lerp(minPitch, maxPitch, currentSpeed / (float)maxSpeed.Get(this.gameObject));
        audioSource.volume = Mathf.Lerp(minVolume, maxVolume, currentSpeed / (float)maxSpeed.Get(this.gameObject));
        breakingAudioSource.volume = Mathf.Lerp(minVolume, maxVolume, currentSpeed / (float)maxSpeed.Get(this.gameObject));

        float rotation = Mathf.Clamp(currentSpeed / (float)maxSpeed.Get(this.gameObject), 0.0f, 1.0f) * 180.0f;
        speedometer.transform.rotation = Quaternion.Euler(0.0f, 0.0f, -rotation);
    }
}
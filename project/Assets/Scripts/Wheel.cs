using UnityEngine;

public class Wheel : MonoBehaviour
{

    public AudioSource driftSound;
    public bool inverseSteer, power, steer;
    public float steerAngle, torque;
    public ParticleSystem driftSmoke;
    public Rigidbody carBody;
    public Transform wheelTransform;
    private float carForwardVelocity, driftIntensity, lastFixedUpdateTime, maxDriftIntensity, minDriftSpeed, totalDrift, wheelAngularVelocity, wheelSlipMultiplier, wheelSpin;
    private int lastDriftIndex;
    private ParticleSystem.EmissionModule smokeEmission;
    private Quaternion wheelRotation;
    private Vector3 driftPoint, localVelocity, wheelPosition;
    private WheelCollider wheelCollider;
    private WheelHit wheelHitInfo;

    private void Awake()
    {
        minDriftSpeed = 0.5f;
        maxDriftIntensity = 10f;
        wheelSlipMultiplier = 1000f;
        lastDriftIndex = -1;
    }

    private void Start()
    {
        wheelCollider = GetComponentInChildren<WheelCollider>();
        wheelTransform = GetComponentInChildren<MeshRenderer>().GetComponent<Transform>();
        lastFixedUpdateTime = Time.time;
        smokeEmission = driftSmoke.emission;
        driftIntensity = 0f;
    }

    private void Update()
    {
        wheelCollider.GetWorldPose(out wheelPosition, out wheelRotation);
        wheelTransform.position = wheelPosition;
        wheelTransform.rotation = wheelRotation;
    }

    private void FixedUpdate()
    {
        if (power)
        { wheelCollider.motorTorque = torque; }
        if (steer)
        { wheelCollider.steerAngle = steerAngle * (inverseSteer ? -1 : 1); }
        lastFixedUpdateTime = Time.time;
    }

    private void LateUpdate()
    {
        if (wheelCollider.GetGroundHit(out wheelHitInfo))
        {
            GetWheelInfo();
            if (totalDrift >= minDriftSpeed)
            {
                driftIntensity = Mathf.Clamp((totalDrift / maxDriftIntensity), 0, 1);
                driftPoint = wheelHitInfo.point + transform.forward + (carBody.velocity * (Time.time - lastFixedUpdateTime));
                lastDriftIndex = GameManager.instance.driftController.AddDriftMark(driftPoint, wheelHitInfo.normal, driftIntensity, lastDriftIndex);
                if ((driftIntensity > 0.5f) && (Mathf.Abs(carForwardVelocity) > 2f))
                { 
                    smokeEmission.rateOverTime = (driftIntensity * 100f) - 50f;
                    if (!driftSound.isPlaying) { driftSound.Play();}
                }
                driftSound.volume = driftIntensity;
            }
            else { NoDrift(); }
        }
        else { NoDrift(); }
    }

    private void GetWheelInfo()
    {
        localVelocity = transform.InverseTransformDirection(carBody.velocity);
        totalDrift = Mathf.Abs(localVelocity.x);
        wheelAngularVelocity = wheelCollider.radius * ((2f * Mathf.PI * wheelCollider.rpm) / 60f);
        carForwardVelocity = Vector3.Dot(carBody.velocity, transform.forward);
        wheelSpin = Mathf.Abs(carForwardVelocity - wheelAngularVelocity) * wheelSlipMultiplier;
        wheelSpin = Mathf.Max(0f, wheelSpin * (5f - Mathf.Abs(carForwardVelocity)));
        totalDrift += wheelSpin;
    }

    private void NoDrift()
    { smokeEmission.rateOverTime = 0f; lastDriftIndex = -1; }

}
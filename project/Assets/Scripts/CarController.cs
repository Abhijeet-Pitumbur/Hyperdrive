using UnityEngine;

public class CarController : MonoBehaviour
{

    public float maxSteer, maxThrottle, steer, throttle;
    public Transform centerOfMass;
    private Rigidbody carBody;
    private Wheel[] wheels;

    private void Start()
    {
        maxThrottle = 2000f;
        maxSteer = 17f;
        wheels = GetComponentsInChildren<Wheel>();
        carBody = GetComponent<Rigidbody>();
        carBody.centerOfMass = centerOfMass.localPosition;
    }

    private void Update()
    {
        foreach (Wheel wheel in wheels)
        {
            wheel.torque = throttle * maxThrottle;
            wheel.steerAngle = steer * maxSteer;
        }
    }

}
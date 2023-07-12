using UnityEngine;

public class InputControllerB : MonoBehaviour
{

    public float steerInputB, throttleInputB;
    private string inputSteerAxisB, inputThrottleAxisB;

    private void Awake()
    { inputSteerAxisB = "HorizontalB"; inputThrottleAxisB = "VerticalB"; }

    private void Update()
    {
        steerInputB = Input.GetAxis(inputSteerAxisB);
        throttleInputB = Input.GetAxis(inputThrottleAxisB);
    }

}
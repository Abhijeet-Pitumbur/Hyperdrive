using UnityEngine;

public class InputController : MonoBehaviour
{

    public float steerInput, throttleInput;
    private string inputSteerAxis, inputThrottleAxis;

    private void Awake()
    { inputSteerAxis = "Horizontal"; inputThrottleAxis = "Vertical"; }

    private void Update()
    {
        steerInput = Input.GetAxis(inputSteerAxis);
        throttleInput = Input.GetAxis(inputThrottleAxis);
    }

}
using System.Collections;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{

    private bool inFrontCar, animating;
    private float currentHeight, currentRotationAngle, distance, height, heightDamping, rotationDamping, wantedHeight, wantedRotationAngle;
    private Quaternion currentRotation;
    private Transform humanCar;

    private void Start()
    {
        animating = false;
        inFrontCar = false;
        distance = 10f;
        height = 7f;
        heightDamping = 3f;
        rotationDamping = 3f;
        humanCar = GameManager.instance.humanCar;
        InstantiateCamera();
    }

    private void LateUpdate()
    {
        if (GameManager.instance.UIController.cameraSwirl)
        { inFrontCar = false; BesideCar(); }
        else if ((inFrontCar) && (!GameManager.instance.UIController.cameraSwirl))
        { InFrontCar(); }
        else
        { BehindCar(); }
        MoveCamera();
    }

    private void BesideCar()
    {
        wantedRotationAngle = humanCar.eulerAngles.y - 150f;
        wantedHeight = Mathf.Abs(humanCar.position.y) * (height - 1f);
        if (wantedHeight > 100f) { wantedHeight = 100f; }
    }

    private void BehindCar()
    {
        wantedRotationAngle = humanCar.eulerAngles.y;
        wantedHeight = Mathf.Abs(humanCar.position.y) * height;
        if (wantedHeight > 750f) { wantedHeight = 750f; }
    }

    private void InFrontCar()
    {
        wantedRotationAngle = humanCar.eulerAngles.y;
        wantedHeight = Mathf.Abs(humanCar.position.y) * (height - 4f);
        if (wantedHeight > 750f) { wantedHeight = 750f; }
    }

    private void MoveCamera()
    {
        if (inFrontCar)
        { currentRotationAngle = Mathf.LerpAngle(currentRotationAngle, wantedRotationAngle, 3f * rotationDamping * Time.unscaledDeltaTime); }
        else
        { currentRotationAngle = Mathf.LerpAngle(currentRotationAngle, wantedRotationAngle, rotationDamping * Time.unscaledDeltaTime); }
        currentHeight = Mathf.Lerp(currentHeight, wantedHeight, heightDamping * Time.unscaledDeltaTime);
        currentRotation = Quaternion.Euler(0f, currentRotationAngle, 0f);
        transform.position = humanCar.position;
        transform.position -= currentRotation * Vector3.forward * distance;
        transform.position = new Vector3 (transform.position.x, currentHeight, transform.position.z);
        transform.LookAt(humanCar);
        if (inFrontCar)
        { transform.position += currentRotation * Vector3.forward * (distance + 4f); }
    }

    private void InstantiateCamera()
    {
        BesideCar();
        currentRotationAngle = wantedRotationAngle;
        currentHeight = wantedHeight;
        currentRotation = Quaternion.Euler(0f, currentRotationAngle, 0f);
        transform.position = humanCar.position;
        transform.position -= currentRotation * Vector3.forward * distance;
        transform.position = new Vector3 (transform.position.x, currentHeight, transform.position.z);
        transform.LookAt(humanCar);
    }

    private IEnumerator ChangeCameraView()
    {
        if (animating) { yield break; }
        animating = true;
        inFrontCar = !inFrontCar;
        yield return new WaitForSecondsRealtime(1f);
        animating = false;
    }

    public void ChangeCameraViewWrapper()
    { StartCoroutine(ChangeCameraView()); }

}
using System.Collections;
using UnityEngine;

public class CameraFollowB : MonoBehaviour
{

    private bool inFrontCarB, animatingB;
    private float currentHeightB, currentRotationAngleB, distanceB, heightB, heightDampingB, rotationDampingB, wantedHeightB, wantedRotationAngleB;
    private Quaternion currentRotationB;
    private Transform humanCarB;

    private void Start()
    {
        animatingB = false;
        inFrontCarB = false;
        distanceB = 10f;
        heightB = 7f;
        heightDampingB = 3f;
        rotationDampingB = 3f;
        humanCarB = GameManager.instance.humanCarB;
        InstantiateCameraB();
    }

    private void LateUpdate()
    {
        if (GameManager.instance.UIController.cameraSwirlB)
        { inFrontCarB = false; BesideCarB(); }
        else if ((inFrontCarB) && (!GameManager.instance.UIController.cameraSwirlB))
        { InFrontCarB(); }
        else
        { BehindCarB(); }
        MoveCameraB();
    }

    private void BesideCarB()
    {
        wantedRotationAngleB = humanCarB.eulerAngles.y - 150f;
        wantedHeightB = Mathf.Abs(humanCarB.position.y) * (heightB - 1f);
        if (wantedHeightB > 100f) { wantedHeightB = 100f; }
    }

    private void BehindCarB()
    {
        wantedRotationAngleB = humanCarB.eulerAngles.y;
        wantedHeightB = Mathf.Abs(humanCarB.position.y) * heightB;
        if (wantedHeightB > 750f) { wantedHeightB = 750f; }
    }

    private void InFrontCarB()
    {
        wantedRotationAngleB = humanCarB.eulerAngles.y;
        wantedHeightB = Mathf.Abs(humanCarB.position.y) * (heightB - 4f);
        if (wantedHeightB > 750f) { wantedHeightB = 750f; }
    }

    private void MoveCameraB()
    {
        if (inFrontCarB)
        { currentRotationAngleB = Mathf.LerpAngle(currentRotationAngleB, wantedRotationAngleB, 3f * rotationDampingB * Time.unscaledDeltaTime); }
        else
        { currentRotationAngleB = Mathf.LerpAngle(currentRotationAngleB, wantedRotationAngleB, rotationDampingB * Time.unscaledDeltaTime); }
        currentHeightB = Mathf.Lerp(currentHeightB, wantedHeightB, heightDampingB * Time.unscaledDeltaTime);
        currentRotationB = Quaternion.Euler(0f, currentRotationAngleB, 0f);
        transform.position = humanCarB.position;
        transform.position -= currentRotationB * Vector3.forward * distanceB;
        transform.position = new Vector3 (transform.position.x, currentHeightB, transform.position.z);
        transform.LookAt(humanCarB);
        if (inFrontCarB)
        { transform.position += currentRotationB * Vector3.forward * (distanceB + 4f); }
    }

    private void InstantiateCameraB()
    {
        BesideCarB();
        currentRotationAngleB = wantedRotationAngleB;
        currentHeightB = wantedHeightB;
        currentRotationB = Quaternion.Euler(0f, currentRotationAngleB, 0f);
        transform.position = humanCarB.position;
        transform.position -= currentRotationB * Vector3.forward * distanceB;
        transform.position = new Vector3 (transform.position.x, currentHeightB, transform.position.z);
        transform.LookAt(humanCarB);
    }

    private IEnumerator ChangeCameraViewB()
    {
        if (animatingB) { yield break; }
        animatingB = true;
        inFrontCarB = !inFrontCarB;
        yield return new WaitForSecondsRealtime(1f);
        animatingB = false;
    }

    public void ChangeCameraViewWrapperB()
    { StartCoroutine(ChangeCameraViewB()); }

}
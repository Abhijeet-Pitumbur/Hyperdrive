using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CarSelectionB : MonoBehaviour
{

    private GameObject carLockObjectB;
    private Image carLockImageB;
    private WaitForSecondsRealtime tenthSecondDelay, quarterSecondDelay, twoFifthsSecondDelay;

    public void SelectCarB()
    {
        Menu.ins.menuScreen = Menu.MenuScreens.carsB;
        Menu.ins.carSelectionImageB.transform.localScale = new Vector3(1f, 1f, 1f);
        Menu.ins.carSelectionImageB.LeanScale(new Vector3(0f, 0f, 0f), 1.5f).setEaseInOutQuart().setIgnoreTimeScale(true);;
        Menu.ins.FadeIn(Menu.ins.carSelectionCanvasB);
        StartCoroutine(Menu.ins.FadeOut(Menu.ins.carSelectionCanvas));
        if (Menu.ins.carsB.Count <= 0)
        {
            foreach (Transform carB in Menu.ins.carsParentB.transform)
            { Menu.ins.carsB.Add(carB); }
        }
        Menu.ins.carsB[Menu.ins.selectedCarB].rotation = Menu.ins.cars[Menu.ins.selectedCar].rotation;
        Menu.ins.carsParent.SetActive(false);
        Menu.ins.carsParentB.SetActive(true);
        if (Menu.ins.carsInstantiatedB) { CheckCarLockB(); return; }
        tenthSecondDelay = new WaitForSecondsRealtime(0.1f);
        quarterSecondDelay = new WaitForSecondsRealtime(0.25f);
        twoFifthsSecondDelay = new WaitForSecondsRealtime(0.4f);
        carLockObjectB = (Menu.ins.carLockTextB.gameObject.transform.parent).gameObject;
        carLockImageB = carLockObjectB.GetComponent<Image>();
        carLockObjectB.transform.localScale = new Vector3(0f, 0f, 0f);
        Menu.ins.selectedCarB = 0;
        for (int i = 0; i < Menu.ins.totalNumCars; i ++)
        { Menu.ins.carsB[i].position = new Vector3(Menu.ins.carsB[i].position.x, Menu.ins.carsB[i].position.y, Menu.ins.nextCarPosition); }
        Menu.ins.carsB[Menu.ins.selectedCarB].position = new Vector3(Menu.ins.carsB[Menu.ins.selectedCarB].position.x, Menu.ins.carsB[Menu.ins.selectedCarB].position.y, Menu.ins.currentCarPosition);
        Menu.ins.carNameB.text = Menu.ins.carsB[Menu.ins.selectedCarB].gameObject.name;
        Menu.ins.previousCarButtonB.interactable = false;
        if (Menu.ins.totalNumCars > 1)
        { Menu.ins.nextCarButtonB.interactable = true; }
        else
        { Menu.ins.nextCarButtonB.interactable = false; }
        CheckCarLockB();
        Menu.ins.carsInstantiatedB = true;
    }

    public IEnumerator NextCarB()
    {
        Menu.ins.animating = true;
        Menu.ins.carsB[Menu.ins.selectedCarB].LeanMoveZ(Menu.ins.previousCarPosition, Menu.ins.halfSecond).setEaseInOutQuart().setIgnoreTimeScale(true);
        Menu.ins.selectedCarB++;
        Menu.ins.carsB[Menu.ins.selectedCarB].rotation = Quaternion.Euler(0f, -90f, 0f);
        Menu.ins.carsB[Menu.ins.selectedCarB].LeanMoveZ(Menu.ins.currentCarPosition, Menu.ins.halfSecond).setEaseInOutQuart().setIgnoreTimeScale(true);
        Menu.ins.previousCarButtonB.interactable = true;
        if (Menu.ins.selectedCarB == Menu.ins.totalNumCars - 1)
        { Menu.ins.nextCarButtonB.interactable = false; }
        CheckCarLockB();
        Menu.ins.carNameB.gameObject.LeanScaleX(0f, 0.1f).setEaseInOutQuart().setIgnoreTimeScale(true);;
        yield return tenthSecondDelay;
        Menu.ins.carNameB.text = Menu.ins.carsB[Menu.ins.selectedCarB].gameObject.name;
        Menu.ins.carNameB.gameObject.LeanScaleX(1f, 0.1f).setEaseInOutQuart().setIgnoreTimeScale(true);;
        yield return twoFifthsSecondDelay;
        Menu.ins.animating = false;
    }

    public IEnumerator PreviousCarB()
    {
        Menu.ins.animating = true;
        Menu.ins.carsB[Menu.ins.selectedCarB].LeanMoveZ(Menu.ins.nextCarPosition, Menu.ins.halfSecond).setEaseInOutQuart().setIgnoreTimeScale(true);
        Menu.ins.selectedCarB--;
        Menu.ins.carsB[Menu.ins.selectedCarB].rotation = Quaternion.Euler(0f, -90f, 0f);
        Menu.ins.carsB[Menu.ins.selectedCarB].LeanMoveZ(Menu.ins.currentCarPosition, Menu.ins.halfSecond).setEaseInOutQuart().setIgnoreTimeScale(true);
        Menu.ins.nextCarButtonB.interactable = true;
        if (Menu.ins.selectedCarB == 0)
        { Menu.ins.previousCarButtonB.interactable = false; }
        CheckCarLockB();
        Menu.ins.carNameB.gameObject.LeanScaleX(0f, 0.1f).setEaseInOutQuart().setIgnoreTimeScale(true);;
        yield return tenthSecondDelay;
        Menu.ins.carNameB.text = Menu.ins.carsB[Menu.ins.selectedCarB].gameObject.name;
        Menu.ins.carNameB.gameObject.LeanScaleX(1f, 0.1f).setEaseInOutQuart().setIgnoreTimeScale(true);;
        yield return twoFifthsSecondDelay;
        Menu.ins.animating = false;
    }

    private void CheckCarLockB()
    {
        if (Menu.ins.selectedCar == Menu.ins.selectedCarB) { LockCarB(); }
        else { StartCoroutine(UnlockCarB()); }
    }

    private void LockCarB()
    {
        Menu.ins.playButtonCarB.interactable = false;
        carLockObjectB.SetActive(true);
        carLockObjectB.LeanScale(new Vector3(1f, 1f, 1f), 0.25f).setEaseInOutQuart().setIgnoreTimeScale(true);;
    }

    private IEnumerator UnlockCarB()
    {
        Menu.ins.playButtonCarB.interactable = true;
        carLockObjectB.LeanScale(new Vector3(0f, 0f, 0f), 0.25f).setEaseInOutQuart().setIgnoreTimeScale(true);;
        yield return quarterSecondDelay;
        carLockObjectB.SetActive(false);
    }

    public void NextCarWrapperB() { if (!Menu.ins.animating) { StartCoroutine(NextCarB()); } }
    public void PreviousCarWrapperB() { if (!Menu.ins.animating) { StartCoroutine(PreviousCarB()); } }

}
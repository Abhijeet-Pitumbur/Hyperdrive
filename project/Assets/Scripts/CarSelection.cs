using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CarSelection : MonoBehaviour
{

    private GameObject carLockObject;
    private Image carLockImage;
    private WaitForSecondsRealtime tenthSecondDelay, quarterSecondDelay, twoFifthsSecondDelay;

    public void SelectCar()
    {
        if (Menu.ins.gameType == Menu.GameTypes.play)
        {
            Menu.ins.playButtonCar.gameObject.SetActive(true);
            Menu.ins.nextButtonCar.gameObject.SetActive(false);
            Menu.ins.backButtonCarPlay.gameObject.SetActive(true);
            Menu.ins.backButtonCarQuickRace.gameObject.SetActive(false);
            Menu.ins.carSelectionTextA.SetActive(false);
            Menu.ins.carSelectionImageA.SetActive(false);
        }
        else if (Menu.ins.gameType == Menu.GameTypes.quickRace1P)
        {
            Menu.ins.playButtonCar.gameObject.SetActive(true);
            Menu.ins.nextButtonCar.gameObject.SetActive(false);
            Menu.ins.backButtonCarPlay.gameObject.SetActive(false);
            Menu.ins.backButtonCarQuickRace.gameObject.SetActive(true);
            Menu.ins.carSelectionTextA.SetActive(false);
            Menu.ins.carSelectionImageA.SetActive(false);
        }
        else if (Menu.ins.gameType == Menu.GameTypes.quickRace2P)
        {
            Menu.ins.playButtonCar.gameObject.SetActive(false);
            Menu.ins.nextButtonCar.gameObject.SetActive(true);
            Menu.ins.backButtonCarPlay.gameObject.SetActive(false);
            Menu.ins.backButtonCarQuickRace.gameObject.SetActive(true);
            Menu.ins.carSelectionTextA.SetActive(true);
            Menu.ins.carSelectionImageA.transform.localScale = new Vector3(1f, 1f, 1f);
            Menu.ins.carSelectionImageA.SetActive(true);
            Menu.ins.carSelectionImageA.LeanScale(new Vector3(0f, 0f, 0f), 1.5f).setEaseInOutQuart().setIgnoreTimeScale(true);;
        }
        Menu.ins.cameraAnimation.enabled = false;
        if (Menu.ins.menuScreen != Menu.MenuScreens.carsB)
        {
            Menu.ins.cameraPosition = Menu.ins.mainCamera.position;
            Menu.ins.cameraRotation = Menu.ins.mainCamera.eulerAngles;
        }
        Menu.ins.FadeIn(Menu.ins.carSelectionCanvas);
        if (Menu.ins.menuScreen == Menu.MenuScreens.levels)
        {
            Menu.ins.SetRaceSettings();
            Menu.ins.menuScreen = Menu.MenuScreens.cars;
            StartCoroutine(Menu.ins.FadeOut(Menu.ins.levelSelectionCanvas));
        }
        else if (Menu.ins.menuScreen == Menu.MenuScreens.settings)
        {
            Menu.ins.SetRaceSettings();
            Menu.ins.menuScreen = Menu.MenuScreens.cars;
            StartCoroutine(Menu.ins.FadeOut(Menu.ins.raceSettingsCanvas));
        }
        else if (Menu.ins.menuScreen == Menu.MenuScreens.carsB)
        {
            Menu.ins.menuScreen = Menu.MenuScreens.cars;
            StartCoroutine(Menu.ins.FadeOut(Menu.ins.carSelectionCanvasB));
        }
        Menu.ins.mainCamera.LeanMove(new Vector3(7f, 2.5f, 2f), Menu.ins.halfSecond).setEaseInOutQuart().setIgnoreTimeScale(true);
        Menu.ins.mainCamera.LeanRotate(new Vector3(15f, 253f, 0f), Menu.ins.halfSecond).setEaseInOutQuart().setIgnoreTimeScale(true);
        if (Menu.ins.cars.Count <= 0)
        {
            foreach (Transform car in Menu.ins.carsParent.transform)
            { Menu.ins.cars.Add(car); }
        }
        if (Menu.ins.carsInstantiatedB)
        { Menu.ins.cars[Menu.ins.selectedCar].rotation = Menu.ins.carsB[Menu.ins.selectedCarB].rotation; }
        else
        { Menu.ins.cars[Menu.ins.selectedCar].rotation = Quaternion.Euler(0f, -90f, 0f); }
        Menu.ins.carsParentB.SetActive(false);
        Menu.ins.carsParent.SetActive(true);
        if (Menu.ins.selectedCar != 0) { CheckCarLock(); }
        if (Menu.ins.carsInstantiated) { return; }
        Menu.ins.previousCarPosition = -30f;
        Menu.ins.currentCarPosition = 0f;
        Menu.ins.nextCarPosition = 30f;
        tenthSecondDelay = new WaitForSecondsRealtime(0.1f);
        quarterSecondDelay = new WaitForSecondsRealtime(0.25f);
        twoFifthsSecondDelay = new WaitForSecondsRealtime(0.4f);
        carLockObject = (Menu.ins.carLockText.gameObject.transform.parent).gameObject;
        carLockImage = carLockObject.GetComponent<Image>();
        carLockObject.transform.localScale = new Vector3(0f, 0f, 0f);
        Menu.ins.selectedCar = 0;
        Menu.ins.totalNumCars = Menu.ins.cars.Count;
        for (int i = 0; i < Menu.ins.totalNumCars; i ++)
        { Menu.ins.cars[i].position = new Vector3(Menu.ins.cars[i].position.x, Menu.ins.cars[i].position.y, Menu.ins.nextCarPosition); }
        Menu.ins.cars[Menu.ins.selectedCar].position = new Vector3(Menu.ins.cars[Menu.ins.selectedCar].position.x, Menu.ins.cars[Menu.ins.selectedCar].position.y, Menu.ins.currentCarPosition);
        Menu.ins.carName.text = Menu.ins.cars[Menu.ins.selectedCar].gameObject.name;
        Menu.ins.previousCarButton.interactable = false;
        if (Menu.ins.totalNumCars > 1)
        { Menu.ins.nextCarButton.interactable = true; }
        else
        { Menu.ins.nextCarButton.interactable = false; }
        for (int i = 0; i <= 1; i++)
        { Menu.ins.carLockTexts[i] = string.Empty; }
        Menu.ins.carLockTexts[2] = "Get 3 stars in \n Level 1 to unlock";
        Menu.ins.carLockTexts[3] = "Get 3 stars in \n Level 3 to unlock";
        Menu.ins.carLockTexts[4] = "Get 3 stars in \n Level 5 to unlock";
        CheckCarLock();
        Menu.ins.carsInstantiated = true;
    }

    public IEnumerator NextCar()
    {
        Menu.ins.animating = true;
        Menu.ins.cars[Menu.ins.selectedCar].LeanMoveZ(Menu.ins.previousCarPosition, Menu.ins.halfSecond).setEaseInOutQuart().setIgnoreTimeScale(true);
        Menu.ins.selectedCar++;
        Menu.ins.cars[Menu.ins.selectedCar].rotation = Quaternion.Euler(0f, -90f, 0f);
        Menu.ins.cars[Menu.ins.selectedCar].LeanMoveZ(Menu.ins.currentCarPosition, Menu.ins.halfSecond).setEaseInOutQuart().setIgnoreTimeScale(true);
        Menu.ins.previousCarButton.interactable = true;
        if (Menu.ins.selectedCar == Menu.ins.totalNumCars - 1)
        { Menu.ins.nextCarButton.interactable = false; }
        CheckCarLock();
        Menu.ins.carName.gameObject.LeanScaleX(0f, 0.1f).setEaseInOutQuart().setIgnoreTimeScale(true);;
        yield return tenthSecondDelay;
        Menu.ins.carName.text = Menu.ins.cars[Menu.ins.selectedCar].gameObject.name;
        Menu.ins.carName.gameObject.LeanScaleX(1f, 0.1f).setEaseInOutQuart().setIgnoreTimeScale(true);;
        yield return twoFifthsSecondDelay;
        Menu.ins.animating = false;
    }

    public IEnumerator PreviousCar()
    {
        Menu.ins.animating = true;
        Menu.ins.cars[Menu.ins.selectedCar].LeanMoveZ(Menu.ins.nextCarPosition, Menu.ins.halfSecond).setEaseInOutQuart().setIgnoreTimeScale(true);
        Menu.ins.selectedCar--;
        Menu.ins.cars[Menu.ins.selectedCar].rotation = Quaternion.Euler(0f, -90f, 0f);
        Menu.ins.cars[Menu.ins.selectedCar].LeanMoveZ(Menu.ins.currentCarPosition, Menu.ins.halfSecond).setEaseInOutQuart().setIgnoreTimeScale(true);
        Menu.ins.nextCarButton.interactable = true;
        if (Menu.ins.selectedCar == 0)
        { Menu.ins.previousCarButton.interactable = false; }
        CheckCarLock();
        Menu.ins.carName.gameObject.LeanScaleX(0f, 0.1f).setEaseInOutQuart().setIgnoreTimeScale(true);;
        yield return tenthSecondDelay;
        Menu.ins.carName.text = Menu.ins.cars[Menu.ins.selectedCar].gameObject.name;
        Menu.ins.carName.gameObject.LeanScaleX(1f, 0.1f).setEaseInOutQuart().setIgnoreTimeScale(true);;
        yield return twoFifthsSecondDelay;
        Menu.ins.animating = false;
    }

    private void CheckCarLock()
    {
        if (Menu.ins.gameType == Menu.GameTypes.play)
        {
            if (Menu.ins.selectedCar <= 1) { PlayerPrefs.SetInt("Car" + Menu.ins.selectedCar.ToString(), 1); }
            if (PlayerPrefs.GetInt("Car" + Menu.ins.selectedCar.ToString(), 0) == 0)
            { LockCar(); }
            else
            { StartCoroutine(UnlockCar()); }
        }
        else
        { StartCoroutine(UnlockCar()); }
    }

    private void LockCar()
    {
        Menu.ins.playButtonCar.interactable = false;
        Menu.ins.carLockText.text = Menu.ins.carLockTexts[Menu.ins.selectedCar];
        carLockObject.SetActive(true);
        carLockObject.LeanScale(new Vector3(1f, 1f, 1f), 0.25f).setEaseInOutQuart().setIgnoreTimeScale(true);;
    }

    private IEnumerator UnlockCar()
    {
        Menu.ins.playButtonCar.interactable = true;
        carLockObject.LeanScale(new Vector3(0f, 0f, 0f), 0.25f).setEaseInOutQuart().setIgnoreTimeScale(true);;
        yield return quarterSecondDelay;
        carLockObject.SetActive(false);
        Menu.ins.carLockText.text = string.Empty;
    }

    public void NextCarWrapper() { if (!Menu.ins.animating) { StartCoroutine(NextCar()); } }
    public void PreviousCarWrapper() { if (!Menu.ins.animating) { StartCoroutine(PreviousCar()); } }

}
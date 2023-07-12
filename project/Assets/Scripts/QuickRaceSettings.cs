using System.Collections;
using UnityEngine;

public class QuickRaceSettings : MonoBehaviour
{

    public void SetQuickRaceSettingsWrapper() { StartCoroutine(SetQuickRaceSettings()); }

    private IEnumerator SetQuickRaceSettings()
    {
        if (Menu.ins.gameType == Menu.GameTypes.quickRace2P)
        {
            if (Menu.ins.raceSettings1Car.isOn) { Menu.ins.raceSettings2Cars.isOn = true; }
            Menu.ins.raceSettings1Car.interactable = false;
            Menu.ins.backButtonSettingsQuickRace2P.gameObject.SetActive(true);
            Menu.ins.backButtonSettingsQuickRace1P.gameObject.SetActive(false);
        }
        else
        {
            Menu.ins.raceSettings1Car.interactable = true;
            Menu.ins.backButtonSettingsQuickRace2P.gameObject.SetActive(false);
            Menu.ins.backButtonSettingsQuickRace1P.gameObject.SetActive(true);
        }
        Menu.ins.FadeIn(Menu.ins.raceSettingsCanvas);
        Menu.ins.carsParent.SetActive(false);
        if (Menu.ins.menuScreen == Menu.MenuScreens.levels)
        {
            Menu.ins.SetRaceSettings();
            Menu.ins.menuScreen = Menu.MenuScreens.settings;
            Menu.ins.mainCars.SetActive(false);
            StartCoroutine(Menu.ins.FadeOut(Menu.ins.levelSelectionCanvas));
        }
        else if (Menu.ins.menuScreen == Menu.MenuScreens.cars)
        {
            Menu.ins.menuScreen = Menu.MenuScreens.settings;
            StartCoroutine(Menu.ins.FadeOut(Menu.ins.carSelectionCanvas));
            Menu.ins.mainCamera.LeanMove(Menu.ins.cameraPosition, Menu.ins.halfSecond).setEaseInOutQuart().setIgnoreTimeScale(true);
            Menu.ins.mainCamera.LeanRotate(Menu.ins.cameraRotation, Menu.ins.halfSecond).setEaseInOutQuart().setIgnoreTimeScale(true);
            yield return new WaitForSecondsRealtime(0.5f);
            Menu.ins.cameraAnimation.enabled = true;
        }
        if (Menu.ins.carsInstantiatedB)
        { Menu.ins.carsB[Menu.ins.selectedCarB].rotation = Quaternion.Euler(0f, -90f, 0f); }
    }

}
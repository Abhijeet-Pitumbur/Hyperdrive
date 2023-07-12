using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelection : MonoBehaviour
{

    private Transform level, levelStars, levelLock, levelMode;
    private Toggle levelToggle;
    private int numStars;

    public void SelectLevelWrapper() { StartCoroutine(SelectLevel()); }

    private IEnumerator SelectLevel()
    {
        if (Menu.ins.gameType == Menu.GameTypes.play)
        {
            Menu.ins.nextButtonLevelPlay.gameObject.SetActive(true);
            Menu.ins.nextButtonLevelQuickRace.gameObject.SetActive(false);
        }
        else
        {
            Menu.ins.nextButtonLevelPlay.gameObject.SetActive(false);
            Menu.ins.nextButtonLevelQuickRace.gameObject.SetActive(true);
        }
        Menu.ins.FadeIn(Menu.ins.levelSelectionCanvas);
        Menu.ins.carsParent.SetActive(false);
        if (Menu.ins.menuScreen == Menu.MenuScreens.main)
        {
            Menu.ins.menuScreen = Menu.MenuScreens.levels;
            Menu.ins.mainCars.SetActive(false);
            StartCoroutine(Menu.ins.FadeOut(Menu.ins.mainMenuCanvas));
        }
        else if (Menu.ins.menuScreen == Menu.MenuScreens.settings)
        {
            Menu.ins.menuScreen = Menu.MenuScreens.levels;
            StartCoroutine(Menu.ins.FadeOut(Menu.ins.raceSettingsCanvas));
            Menu.ins.cameraAnimation.enabled = true;
        }
        else if (Menu.ins.menuScreen == Menu.MenuScreens.cars)
        {
            Menu.ins.menuScreen = Menu.MenuScreens.levels;
            StartCoroutine(Menu.ins.FadeOut(Menu.ins.carSelectionCanvas));
            Menu.ins.mainCamera.LeanMove(Menu.ins.cameraPosition, Menu.ins.halfSecond).setEaseInOutQuart().setIgnoreTimeScale(true);
            Menu.ins.mainCamera.LeanRotate(Menu.ins.cameraRotation, Menu.ins.halfSecond).setEaseInOutQuart().setIgnoreTimeScale(true);
            yield return new WaitForSecondsRealtime(0.5f);
            Menu.ins.cameraAnimation.enabled = true;
        }
        for (int i = 0; i < Menu.ins.totalNumLevels; i++)
        {
            level = Menu.ins.levelsLayout.gameObject.transform.GetChild(i);
            levelStars = level.GetChild(4);
            levelLock = level.GetChild(5);
            levelMode = level.GetChild(6);
            levelToggle = level.GetComponent<Toggle>();
            if (Menu.ins.gameType == Menu.GameTypes.play)
            {
                if (i <= 2) { PlayerPrefs.SetInt("Level" + (i + 1).ToString(), 1); }
                if (PlayerPrefs.GetInt("Level" + (i + 1).ToString(), 0) == 0)
                { CheckSelection(); LockLevel(); }
                else
                { numStars = PlayerPrefs.GetInt("Stars" + (i + 1).ToString(), 0); UnlockLevel(); }
            }
            else
            { QuickRaceLevel(); }
        }
    }

    private void LockLevel()
    {
        levelStars.gameObject.SetActive(false);
        levelLock.gameObject.SetActive(true);
        levelMode.gameObject.SetActive(true);
        levelToggle.interactable = false;
    }

    private void UnlockLevel()
    {
        SetStars();
        levelStars.gameObject.SetActive(true);
        levelLock.gameObject.SetActive(false);
        levelMode.gameObject.SetActive(true);
        levelToggle.interactable = true;
    }

    private void SetStars()
    {
        for (int i = 0; i <= 2; i++)
        { levelStars.GetChild(i).gameObject.SetActive(false); }
        if ((numStars >= 1) && (numStars <= 3))
        { levelStars.GetChild(numStars - 1).gameObject.SetActive(true); }
    }

    private void QuickRaceLevel()
    {
        levelStars.gameObject.SetActive(false);
        levelLock.gameObject.SetActive(false);
        levelMode.gameObject.SetActive(false);
        levelToggle.interactable = true;
    }

    private void CheckSelection()
    {
        if (!levelToggle.isOn) { return; }
        Menu.ins.levelsLayout.transform.GetChild(0).GetComponent<Toggle>().isOn = true;
        Menu.ins.levelScrollbar.value = 0;
    }

}
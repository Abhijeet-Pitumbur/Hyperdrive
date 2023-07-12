using System.Collections.Generic;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine;

public class Menu : MonoBehaviour
{

    public static Menu ins;
    public Animator cameraAnimation;
    public AudioClip menuSong, raceSong;
    public bool animating, carsInstantiated, carsInstantiatedB, windowOpen;
    public Button nextCarButton, previousCarButton, nextCarButtonB, previousCarButtonB, nextButtonLevelPlay, nextButtonLevelQuickRace, backButtonSettingsQuickRace1P, backButtonSettingsQuickRace2P, backButtonCarPlay, backButtonCarQuickRace, playButtonCar, nextButtonCar, playButtonCarB;
    public CanvasGroup carSelectionCanvas, carSelectionCanvasB, exitCanvas, launchCanvas, levelSelectionCanvas, loadingCanvas, mainMenuCanvas, optionsCanvas, raceSettingsCanvas, resetGameCanvas;
    public CarSelection carSelection;
    public CarSelectionB carSelectionB;
    public DepthOfField depthOfField;
    public DifficultyTypes difficultyType;
    public enum DifficultyTypes { Easy, Normal, Hard }
    public enum GameTypes { play, quickRace1P, quickRace2P }
    public enum MapNames { Rural, Artic, Desert, Urban, Moon }
    public enum MenuScreens { main, levels, settings, cars, carsB };
    public enum RaceModes { Classic, Escape, Survival, Infection, Special }
    public float currentCarPosition, halfSecond, nextCarPosition, previousCarPosition, soundVolume;
    public GameObject carsParent, carsParentB, mainCars, carSelectionTextA, carSelectionTextB, carSelectionImageA, carSelectionImageB;
    public GameTypes gameType;
    public int numCars, numLaps, selectedCar, selectedCarB, selectedLevel, totalNumCars, totalNumLevels;
    public LevelSelection levelSelection;
    public List<Transform> cars, carsB;
    public MapNames mapName;
    public MenuScreens menuScreen;
    public MenuWindows menuWindows;
    public Music music;
    public PostProcessVolume postProcessor;
    public RaceModes raceMode;
    public QuickRaceSettings quickRaceSettings;
    public Scrollbar levelScrollbar;
    public Slider volumeSlider;
    public static bool showLoadingCanvas;
    public string[] carLockTexts;
    public TextMeshProUGUI carName, carNameB, carLockText, carLockTextB;
    public Toggle raceSettings1Car, raceSettings2Cars;
    public ToggleGroup levelsLayout, modesLayout, difficultiesLayout, numLapsLayout, numCarsLayout;
    public Transform mainCamera;
    public Vector3 cameraPosition, cameraRotation;
    public WaitForSecondsRealtime halfSecondDelay;

    private void Awake()
    {
        ins = this;
        Time.timeScale = 1f;
        levelSelection = GetComponentInChildren<LevelSelection>();
        quickRaceSettings = GetComponentInChildren<QuickRaceSettings>();
        carSelection = GetComponentInChildren<CarSelection>();
        carSelectionB = GetComponentInChildren<CarSelectionB>();
        menuWindows = GetComponentInChildren<MenuWindows>();
        StartCoroutine(LaunchGame());
    }

    public void Start()
    {
        if (menuScreen == MenuScreens.levels) { StartCoroutine(FadeOut(levelSelectionCanvas)); }
        menuScreen = MenuScreens.main;
        FadeIn(mainMenuCanvas);
        mainCars.SetActive(true);
        cameraAnimation.enabled = true;
        Physics.gravity = new Vector3(0f, -30f, 0f);
    }

    private void LateUpdate()
    {
        if (menuScreen == MenuScreens.cars) { cars[selectedCar].Rotate(0f, 10f * Time.deltaTime, 0f); }
        else if (menuScreen == MenuScreens.carsB) { carsB[selectedCarB].Rotate(0f, 10f * Time.deltaTime, 0f); }
        if (animating) { return; }
        switch (menuScreen)
        {
            case MenuScreens.levels: LevelMenuKeyPress(); break;
            case MenuScreens.settings: RaceSettingsMenuKeyPress(); break;
            case MenuScreens.cars: CarMenuKeyPress(); break;
            case MenuScreens.carsB: CarBMenuKeyPress(); break;
            default: MainMenuKeyPress(); break;
        }
    }

    private IEnumerator LaunchGame()
    {
        animating = true;
        if (showLoadingCanvas)
        {
            loadingCanvas.alpha = 1f;
            loadingCanvas.gameObject.SetActive(true);
        }
        soundVolume = PlayerPrefs.GetFloat("Volume", 2f);
        if ((soundVolume < 0f) || (soundVolume > 1f))
        { soundVolume = 1f; }
        AudioListener.volume = volumeSlider.value = soundVolume;
        cameraAnimation.Play("MenuCamera", -1, 0f);
        totalNumLevels = levelsLayout.gameObject.transform.childCount;
        halfSecond = 0.5f;
        halfSecondDelay = new WaitForSecondsRealtime(halfSecond);
        windowOpen = false;
        carsInstantiated = false;
        carsInstantiatedB = false;
        selectedCar = 0;
        launchCanvas.alpha = 1f;
        launchCanvas.gameObject.SetActive(true);
        launchCanvas.LeanAlpha(0f, 2f).setEaseInOutQuart().setIgnoreTimeScale(true);
        loadingCanvas.LeanAlpha(0f, 1f).setEaseInOutQuart().setIgnoreTimeScale(true);
        animating = true;
        yield return new WaitForSecondsRealtime(2f);
        launchCanvas.gameObject.SetActive(false);
        launchCanvas.alpha = 1f;
        loadingCanvas.gameObject.SetActive(false);
        loadingCanvas.alpha = 1f;
        animating = false;
    }

    private IEnumerator StartRace()
    {
        StartCoroutine(FadeOut(carSelectionCanvas));
        StartCoroutine(FadeOut(carSelectionCanvasB));
        animating = true;
        FadeIn(loadingCanvas);
        Mathf.Clamp(selectedCar, 0, (totalNumCars - 1));
        PlayerPrefs.SetFloat("Volume", soundVolume);
        PlayerPrefs.Save();
        menuSong = music.audioSource.clip;
        music.audioSource.Stop();
        yield return new WaitForSecondsRealtime(2f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + selectedLevel);
        animating = false;
    }

    public void SetRaceSettings()
    {
        if (menuScreen == MenuScreens.levels)
        {
            selectedLevel = int.Parse(levelsLayout.ActiveToggles().FirstOrDefault().gameObject.name);
            Mathf.Clamp(selectedLevel, 1, totalNumLevels);
            switch (selectedLevel)
            {
                case 2: mapName = MapNames.Artic; raceMode = RaceModes.Escape; break;
                case 3: mapName = MapNames.Desert; raceMode = RaceModes.Survival; break;
                case 4: mapName = MapNames.Urban; raceMode = RaceModes.Infection; break;
                case 5: mapName = MapNames.Moon; raceMode = RaceModes.Special; break;
                default: mapName = MapNames.Rural; raceMode = RaceModes.Classic; break;
            }
        }
        switch (gameType)
        {
            case GameTypes.play:
                difficultyType = DifficultyTypes.Normal;
                numLaps = 3;
                numCars = 4;
                break;
            default:
                difficultyType = (DifficultyTypes) int.Parse(difficultiesLayout.ActiveToggles().FirstOrDefault().gameObject.name);
                raceMode = (RaceModes) int.Parse(modesLayout.ActiveToggles().FirstOrDefault().gameObject.name);
                numLaps = int.Parse(numLapsLayout.ActiveToggles().FirstOrDefault().gameObject.name);
                Mathf.Clamp(numLaps, 1, 10);
                numCars = int.Parse(numCarsLayout.ActiveToggles().FirstOrDefault().gameObject.name);
                if (gameType == GameTypes.quickRace1P)
                { Mathf.Clamp(numCars, 2, totalNumCars); }
                else
                { Mathf.Clamp(numCars, 1, totalNumCars); }
                break;
        }
    }

    private void MainMenuKeyPress()
    {
        if (Input.GetKeyDown(KeyCode.Return) && (!windowOpen))
        { ClickPlay(); music.PlayClickSound(); }
        else if (Input.GetKeyDown(KeyCode.Escape) && (!windowOpen))
        { OpenWindow(exitCanvas); music.PlayClickSound(); }
        else if (Input.GetKeyDown(KeyCode.Return) && (exitCanvas.gameObject.activeSelf))
        { ExitGame(); music.PlayClickSound(); }
    }

    private void LevelMenuKeyPress()
    {
        if ((Input.GetKeyDown(KeyCode.Return)) && (gameType == GameTypes.play))
        { SelectCar(); music.PlayClickSound(); }
        else if ((Input.GetKeyDown(KeyCode.Return)) && (gameType != GameTypes.play))
        { SetQuickRaceSettings(); music.PlayClickSound(); }
        else if (Input.GetKeyDown(KeyCode.Escape))
        { Start(); music.PlayClickSound(); }
    }

    private void RaceSettingsMenuKeyPress()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        { SelectCar(); music.PlayClickSound(); }
        else if ((Input.GetKeyDown(KeyCode.Escape)) && (gameType == GameTypes.quickRace1P))
        { ClickQuickRace1P(); music.PlayClickSound(); }
        else if ((Input.GetKeyDown(KeyCode.Escape)) && (gameType == GameTypes.quickRace2P))
        { ClickQuickRace2P(); music.PlayClickSound(); }
    }

    private void CarMenuKeyPress()
    {
        if ((Input.GetKeyDown(KeyCode.Return)) && (gameType != GameTypes.quickRace2P) && (!carLockText.gameObject.activeInHierarchy))
        { StartRaceWrapper(); music.PlayStartSound(); }
        else if ((Input.GetKeyDown(KeyCode.Return)) && (gameType == GameTypes.quickRace2P))
        { SelectCarB(); music.PlayClickSound(); }
        else if ((Input.GetKeyDown(KeyCode.Escape)) && (gameType == GameTypes.play))
        { ClickPlay(); music.PlayClickSound(); }
        else if ((Input.GetKeyDown(KeyCode.Escape)) && (gameType != GameTypes.play))
        { SetQuickRaceSettings(); music.PlayClickSound(); }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) && (selectedCar != 0))
        { PreviousCar(); music.PlayClickSound(); }
        else if (Input.GetKeyDown(KeyCode.RightArrow) && (selectedCar != totalNumCars - 1))
        { NextCar(); music.PlayClickSound(); }
    }

    private void CarBMenuKeyPress()
    {
        if ((Input.GetKeyDown(KeyCode.Return)) && (!carLockTextB.gameObject.activeInHierarchy))
        { StartRaceWrapper(); music.PlayStartSound(); }
        else if (Input.GetKeyDown(KeyCode.Escape))
        { SelectCar(); music.PlayClickSound(); }
        else if (Input.GetKeyDown(KeyCode.A) && (selectedCarB != 0))
        { PreviousCarB(); music.PlayClickSound(); }
        else if (Input.GetKeyDown(KeyCode.D) && (selectedCarB != totalNumCars - 1))
        { NextCarB(); music.PlayClickSound(); }
    }

    private IEnumerator OpenResetGameWindow()
    {
        Menu.ins.animating = true;
        StartCoroutine(Menu.ins.FadeOut(optionsCanvas));
        yield return new WaitForSecondsRealtime(0.2f);
        Menu.ins.FadeIn(resetGameCanvas);
    }

    private IEnumerator CloseResetGameWindow()
    {
        Menu.ins.animating = true;
        StartCoroutine(Menu.ins.FadeOut(resetGameCanvas));
        yield return new WaitForSecondsRealtime(0.2f);
        Menu.ins.FadeIn(optionsCanvas);
    }

    private IEnumerator ResetGame()
    {
        animating = true;
        Menu.showLoadingCanvas = true;
        FadeIn(loadingCanvas);
        yield return new WaitForSecondsRealtime(0.6f);
        PlayerPrefs.DeleteAll();
        SceneManager.LoadScene(0);
        animating = false;
    }

    public void FadeIn(CanvasGroup canvasGroup)
    {
        canvasGroup.alpha = 0f;
        canvasGroup.gameObject.SetActive(true);
        canvasGroup.LeanAlpha(1f, halfSecond).setEaseInOutQuart().setIgnoreTimeScale(true);
    }

    public IEnumerator FadeOut(CanvasGroup canvasGroup)
    {
        animating = true;
        canvasGroup.LeanAlpha(0f, halfSecond).setEaseInOutQuart().setIgnoreTimeScale(true);
        yield return halfSecondDelay;
        canvasGroup.gameObject.SetActive(false);
        canvasGroup.alpha = 1f;
        animating = false;
    }

    public void ClickPlay() { gameType = GameTypes.play; levelSelection.SelectLevelWrapper(); }
    public void ClickQuickRace1P() { gameType = GameTypes.quickRace1P; levelSelection.SelectLevelWrapper(); }
    public void ClickQuickRace2P() { gameType = GameTypes.quickRace2P; levelSelection.SelectLevelWrapper(); }
    public void SetQuickRaceSettings() { quickRaceSettings.SetQuickRaceSettingsWrapper(); }
    public void SelectCar() { carSelection.SelectCar(); }
    public void SelectCarB() { carSelectionB.SelectCarB(); }
    public void NextCar() { carSelection.NextCarWrapper(); }
    public void PreviousCar() { carSelection.PreviousCarWrapper(); }
    public void NextCarB() { carSelectionB.NextCarWrapperB(); }
    public void PreviousCarB() { carSelectionB.PreviousCarWrapperB(); }
    public void StartRaceWrapper() { StartCoroutine(StartRace()); }
    public void OpenWindow(CanvasGroup windowCanvas) { menuWindows.OpenWindow(windowCanvas); }
    public void CloseWindow(CanvasGroup windowCanvas) { menuWindows.CloseWindow(windowCanvas); }
    public void OpenResetGameWindowWrapper() { StartCoroutine(OpenResetGameWindow()); }
    public void CloseResetGameWindowWrapper() { StartCoroutine(CloseResetGameWindow()); }
    public void ResetGameWrapper() { StartCoroutine(ResetGame()); }
    public void ExitGame() { PlayerPrefs.SetFloat("Volume", soundVolume); PlayerPrefs.Save(); Application.Quit(); }

}
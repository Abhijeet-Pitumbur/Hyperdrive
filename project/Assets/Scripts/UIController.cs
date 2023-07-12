using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{

    public AudioClip countdownSound, goSound, finishSound, timerSound, multipleRevSound, singleRevSound, eliminateSound, infectSound, disinfectSound;
    public bool cameraSwirl, cameraSwirlB, raceOver, raceOverB, racePaused, raceStarted, showingInfo;
    public Camera mainCamera, mainCameraB;
    public CanvasGroup buttonsCanvas, buttonsCanvas2P, countdownCanvas, infoCanvas, loadingCanvas, columnCanvas, overCanvas, overCanvasA, overCanvasB, pauseCanvas, raceCanvas, raceCanvasA, raceCanvasB, resultsCanvas, resultsCanvasA, resultsCanvasB, leaderboardCanvasA, leaderboardCanvasB, timerCanvas, quitCanvas, restartCanvas;
    public GameObject unlockText;
    public GameObject[] resultsStars;
    public int numCars;
    public ParticleSystem confetti, confettiB;
    public Player humanCar, humanCarB;
    public PostProcessVolume postProcessor;
    public TextMeshProUGUI countdownText, infoText, leaderboardName, leaderboardTime, leaderboardNameA, leaderboardTimeA, leaderboardNameB, leaderboardTimeB, timerText;
    public TextMeshProUGUI[] currentLapText, carRankText, carSpeedText, currentTimeText, lastLapTimeText, bestLapTimeText, pauseText, resultsText;
    private bool animating, windowOpen;
    private DepthOfField depthOfField;
    private float bestLapTime, bestLapTimeB, carSpeed, carSpeedB, currentLapTime, currentLapTimeB, delayedInfoAppear, infoDisappearTime, halfSecond, lastLapTime, lastLapTimeB;
    private int carRank, carRankB, countdownTime, currentLap, currentLapB, numLaps, selectedLevel;
    private List<Player> cars;
    private Menu.DifficultyTypes difficultyType;
    private Menu.GameTypes gameType;
    private Menu.MapNames mapName;
    private Menu.RaceModes raceMode;
    public Music music;
    private WaitForSecondsRealtime halfSecondDelay;

    private void Awake()
    {
        Time.timeScale = 0f;
        currentLap = 0; currentLapTime = 0f; lastLapTime = 0f; bestLapTime = 0f; carRank = 0;
        currentLapB = 0; currentLapTimeB = 0f; lastLapTimeB = 0f; bestLapTimeB = 0f; carRankB = 0;
        raceStarted = false; racePaused = false; raceOver = false; cameraSwirl = true; cameraSwirlB = true; showingInfo = false;
        halfSecond = 0.5f; halfSecondDelay = new WaitForSecondsRealtime(halfSecond);
    }

    private IEnumerator Start()
    {
        animating = true;
        loadingCanvas.gameObject.SetActive(true);
        buttonsCanvas.gameObject.SetActive(false);
        buttonsCanvas2P.gameObject.SetActive(false);
        countdownCanvas.gameObject.SetActive(false);
        overCanvas.gameObject.SetActive(false);
        pauseCanvas.gameObject.SetActive(false);
        timerCanvas.gameObject.SetActive(false);
        humanCar = GameManager.instance.humanCar.GetComponent<Player>();
        gameType = GameManager.instance.gameType;
        difficultyType = GameManager.instance.difficultyType;
        mapName = GameManager.instance.mapName;
        raceMode = GameManager.instance.raceMode;
        numLaps = GameManager.instance.numLaps;
        numCars = GameManager.instance.numCars;
        selectedLevel = GameManager.instance.selectedLevel;
        if (selectedLevel == 5) { Physics.gravity = new Vector3(0f, -20f, 0f); }
        else { Physics.gravity = new Vector3(0f, -30f, 0f); }
        if (gameType == Menu.GameTypes.quickRace2P)
        {
            raceOverB = false;
            raceCanvasA.alpha = 0f;
            raceCanvasA.gameObject.SetActive(true);
            raceCanvasB.alpha = 0f;
            raceCanvasB.gameObject.SetActive(true);
            columnCanvas.gameObject.SetActive(true);
            humanCarB = GameManager.instance.humanCarB.GetComponent<Player>();
        }
        else
        {
            raceOverB = true;
            raceCanvas.alpha = 0f;
            raceCanvas.gameObject.SetActive(true);
            columnCanvas.gameObject.SetActive(false);
        }
        cars = new List<Player>(humanCar.cars);
        infoDisappearTime = Mathf.Infinity;
        delayedInfoAppear = 0f;
        if ((raceMode == Menu.RaceModes.Escape) || (raceMode == Menu.RaceModes.Survival) || (raceMode == Menu.RaceModes.Infection) || (raceMode == Menu.RaceModes.Special))
        {
            infoCanvas.gameObject.SetActive(true);
            timerCanvas.alpha = 0f;
            timerCanvas.gameObject.SetActive(true);
        }
        else
        {
            infoCanvas.gameObject.SetActive(false);
            timerCanvas.gameObject.SetActive(false);
        }
        switch (gameType)
        {
            case Menu.GameTypes.quickRace1P: pauseText[0].text = "1 Player"; break;
            case Menu.GameTypes.quickRace2P: pauseText[0].text = "2 Players"; break;
            default: pauseText[0].text = ("Level " + selectedLevel.ToString()); break;
        }
        pauseText[1].text = mapName.ToString();
        pauseText[2].text = raceMode.ToString();
        pauseText[3].text = difficultyType.ToString();
        yield return new WaitForSecondsRealtime(0.1f);
        StartCoroutine(FadeOut(loadingCanvas));
        StartCoroutine(StartCountdownTimer());
        StartCoroutine(UpdateSpeed());
        if (numCars > 1)
        { music.audioSource.PlayOneShot(multipleRevSound, 1f); }
        else
        { music.audioSource.PlayOneShot(singleRevSound, 1f); }
        windowOpen = false;
    }

    private void LateUpdate()
    {
        if ((humanCar == null) || ((humanCarB == null) && (gameType == Menu.GameTypes.quickRace2P)))
        { return; }
        if (gameType == Menu.GameTypes.quickRace2P)
        { UpdateRacePanelA(); UpdateRacePanelB(); }
        else
        { UpdateRacePanel(); }
        if ((raceOver) && (gameType != Menu.GameTypes.quickRace2P))
        { UpdateLeaderboardTime(ref leaderboardTime); }
        else if ((raceOver) && (raceOverB) && (gameType == Menu.GameTypes.quickRace2P) && (leaderboardCanvasA.gameObject.activeSelf))
        { UpdateLeaderboardTime(ref leaderboardTimeA); }
        else if ((raceOver) && (raceOverB) && (gameType == Menu.GameTypes.quickRace2P) && (leaderboardCanvasB.gameObject.activeSelf))
        { UpdateLeaderboardTime(ref leaderboardTimeB); }
        if (Time.unscaledTime >= infoDisappearTime)
        {
            infoCanvas.LeanAlpha(0f, halfSecond).setEaseInOutQuart().setIgnoreTimeScale(true);
            infoDisappearTime = Mathf.Infinity;
            showingInfo = false;
        }
        if (animating || windowOpen) { return; }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if ((raceStarted) && (!cameraSwirl) && (racePaused)) { ResumeRace(); }
            else if ((raceStarted) && (!cameraSwirl) && (!racePaused)) { PauseRace(); }
            else if ((gameType == Menu.GameTypes.quickRace2P) && (raceStarted) && (racePaused) && ((!cameraSwirl) || (!cameraSwirlB)))
            { ResumeRace(); }
            else if ((gameType == Menu.GameTypes.quickRace2P) && (raceStarted) && (!racePaused) && ((!cameraSwirl) || (!cameraSwirlB)))
            { PauseRace(); }
            if ((raceOver) && (raceOverB)) { BackToMenuWrapper(); }
        }
        if (Input.GetKeyDown(KeyCode.Return) && (raceOver) && (raceOverB)) { BackToMenuWrapper(); }
    }

    private void UpdateRacePanel()
    {
        if (humanCar.currentLap != currentLap || currentLap == 0)
        {
            currentLap = humanCar.currentLap;
            if (!humanCar.raceOver) { currentLapText[0].text = $"{currentLap} of {numLaps}"; }
        }
        if ((humanCar.carRank != carRank) || (humanCar.numCars != numCars))
        {
            carRank = humanCar.carRank;
            humanCar.numCars = numCars;
            carRankText[0].text = $"{carRank} of {numCars}";
        }
        if (humanCar.currentLapTime != currentLapTime)
        {
            currentLapTime = humanCar.currentLapTime;
            currentTimeText[0].text = $"{(int)currentLapTime/60}:{(currentLapTime)%60:00.00}";
        }
        if (humanCar.lastLapTime != lastLapTime)
        {
            lastLapTime = humanCar.lastLapTime;
            lastLapTimeText[0].text = $"{(int)lastLapTime/60}:{(lastLapTime)%60:00.00}";
        }
        if (humanCar.bestLapTime != bestLapTime)
        {
            bestLapTime = humanCar.bestLapTime;
            bestLapTimeText[0].text = bestLapTime <= 599.99f ? $"{(int)bestLapTime/60}:{(bestLapTime)%60:00.00}" : "0:00.00";
        }
    }

    private void UpdateRacePanelA()
    {
        if (humanCar.currentLap != currentLap || currentLap == 0)
        {
            currentLap = humanCar.currentLap;
            if (!humanCar.raceOver) { currentLapText[1].text = $"{currentLap} of {numLaps}"; }
        }
        if ((humanCar.carRank != carRank) || (humanCar.numCars != numCars))
        {
            carRank = humanCar.carRank;
            humanCar.numCars = numCars;
            carRankText[1].text = $"{carRank} of {numCars}";
        }
        if (humanCar.currentLapTime != currentLapTime)
        {
            currentLapTime = humanCar.currentLapTime;
            currentTimeText[1].text = $"{(int)currentLapTime/60}:{(currentLapTime)%60:00.00}";
        }
        if (humanCar.lastLapTime != lastLapTime)
        {
            lastLapTime = humanCar.lastLapTime;
            lastLapTimeText[1].text = $"{(int)lastLapTime/60}:{(lastLapTime)%60:00.00}";
        }
        if (humanCar.bestLapTime != bestLapTime)
        {
            bestLapTime = humanCar.bestLapTime;
            bestLapTimeText[1].text = bestLapTime <= 599.99f ? $"{(int)bestLapTime/60}:{(bestLapTime)%60:00.00}" : "0:00.00";
        } 
    }

    private void UpdateRacePanelB()
    {
        if (humanCarB.currentLap != currentLapB || currentLapB == 0)
        {
            currentLapB = humanCarB.currentLap;
            if (!humanCarB.raceOver) { currentLapText[2].text = $"{currentLapB} of {numLaps}"; }
        }
        if ((humanCarB.carRank != carRankB) || (humanCarB.numCars != numCars))
        {
            carRankB = humanCarB.carRank;
            humanCarB.numCars = numCars;
            carRankText[2].text = $"{carRankB} of {numCars}";
        }
        if (humanCarB.currentLapTime != currentLapTimeB)
        {
            currentLapTimeB = humanCarB.currentLapTime;
            currentTimeText[2].text = $"{(int)currentLapTimeB/60}:{(currentLapTimeB)%60:00.00}";
        }
        if (humanCarB.lastLapTime != lastLapTimeB)
        {
            lastLapTimeB = humanCarB.lastLapTime;
            lastLapTimeText[2].text = $"{(int)lastLapTimeB/60}:{(lastLapTimeB)%60:00.00}";
        }
        if (humanCarB.bestLapTime != bestLapTimeB)
        {
            bestLapTimeB = humanCarB.bestLapTime;
            bestLapTimeText[2].text = bestLapTimeB <= 599.99f ? $"{(int)bestLapTimeB/60}:{(bestLapTimeB)%60:00.00}" : "0:00.00";
        }
    }

    private IEnumerator StartCountdownTimer()
    {
        animating = true;
        WaitForSecondsRealtime delay = new WaitForSecondsRealtime(1f);
        FadeIn(countdownCanvas);
        Time.timeScale = 0f;
        raceStarted = false;
        countdownTime = 3;
        while (countdownTime > 0)
        {
            countdownText.text = countdownTime.ToString();
            music.audioSource.PlayOneShot(countdownSound);
            yield return delay;
            countdownTime--;
            if (!raceOver) { cameraSwirl = false; }
            if (!raceOverB) { cameraSwirlB = false; }
        }
        countdownText.text = "GO";
        music.audioSource.PlayOneShot(goSound);
        Time.timeScale = 1f;
        if (gameType == Menu.GameTypes.quickRace2P)
        {
            raceCanvasA.LeanAlpha(1f,halfSecond).setEaseInOutQuart().setIgnoreTimeScale(true);
            raceCanvasB.LeanAlpha(1f,halfSecond).setEaseInOutQuart().setIgnoreTimeScale(true);
            FadeIn(buttonsCanvas2P);
        }
        else
        {
            raceCanvas.LeanAlpha(1f,halfSecond).setEaseInOutQuart().setIgnoreTimeScale(true);
            FadeIn(buttonsCanvas);
        }
        if ((raceMode == Menu.RaceModes.Escape) || (raceMode == Menu.RaceModes.Survival) || (raceMode == Menu.RaceModes.Infection) || (raceMode == Menu.RaceModes.Special))
        { timerCanvas.LeanAlpha(1f,halfSecond).setEaseInOutQuart().setIgnoreTimeScale(true); }
        yield return delay;
        StartCoroutine(FadeOut(countdownCanvas));
        if ((raceOver) && (gameType == Menu.GameTypes.quickRace2P))
        { FadeIn(overCanvasA); FadeIn(resultsCanvasA); }
        else if ((raceOverB) && (gameType == Menu.GameTypes.quickRace2P))
        { FadeIn(overCanvasB); FadeIn(resultsCanvasB); }
        racePaused = false;
        raceStarted = true;
    }

    private IEnumerator UpdateSpeed()
    {
        WaitForSeconds delay = new WaitForSeconds(0.1f);
        while ((!raceOver) || (!raceOverB))
        {
            carSpeed =  new Vector3(humanCar.carBody.velocity.x, 0f, humanCar.carBody.velocity.z).magnitude * 3.6f;
            if (carSpeed > 999f) { carSpeed = 999f; }
            if (gameType == Menu.GameTypes.quickRace2P)
            {
                carSpeedText[1].text = $"{(int)carSpeed} km/h";
                carSpeedB =  new Vector3(humanCarB.carBody.velocity.x, 0f, humanCarB.carBody.velocity.z).magnitude * 3.6f;
                if (carSpeedB > 999f) { carSpeedB = 999f; }
                carSpeedText[2].text = $"{(int)carSpeedB} km/h";
            }
            else
            { carSpeedText[0].text = $"{(int)carSpeed} km/h"; }
            yield return delay;
        }
    }

    public void ResumeRace()
    {
        IncreaseDepthOfField();
        StartCoroutine(StartCountdownTimer());
        StartCoroutine(FadeOut(pauseCanvas));
        StartCoroutine(FadeOut(buttonsCanvas));
        StartCoroutine(FadeOut(buttonsCanvas2P));
    }

    public void PauseRace()
    {
        DecreaseDepthOfField();
        FadeIn(pauseCanvas);
        StartCoroutine(FadeOut(buttonsCanvas));
        StartCoroutine(FadeOut(buttonsCanvas2P));
        if (raceOver)
        { StartCoroutine(FadeOut(overCanvasA)); StartCoroutine(FadeOut(resultsCanvasA)); }
        else if (raceOverB)
        { StartCoroutine(FadeOut(overCanvasB)); StartCoroutine(FadeOut(resultsCanvasB)); }
        Time.timeScale = 0f;
        racePaused = true;
    }

    private void DecreaseDepthOfField()
    {
        if (postProcessor.profile.TryGetSettings(out depthOfField))
        { LeanTween.value(6.5f, 1f, halfSecond).setEaseInOutQuart().setIgnoreTimeScale(true).setOnUpdate((float depth) => { depthOfField.focusDistance.value = depth; }); }
    }

    private void IncreaseDepthOfField()
    {
        if (postProcessor.profile.TryGetSettings(out depthOfField))
        { LeanTween.value(1f, 6.5f, halfSecond).setEaseInOutQuart().setIgnoreTimeScale(true).setOnUpdate((float depth) => { depthOfField.focusDistance.value = depth; }); }
    }

    public void IncreaseFieldOfView()
    {
        LeanTween.value(60f, 80f, 1f).setEaseInOutQuart().setIgnoreTimeScale(true).setOnUpdate((float field) => { mainCamera.fieldOfView = field; });
    }

    public void DecreaseFieldOfView()
    {
        LeanTween.value(80f, 60f, 1f).setEaseInOutQuart().setIgnoreTimeScale(true).setOnUpdate((float field) => { mainCamera.fieldOfView = field; });
    }

    public void IncreaseFieldOfViewB()
    {
        LeanTween.value(60f, 80f, 1f).setEaseInOutQuart().setIgnoreTimeScale(true).setOnUpdate((float field) => { mainCameraB.fieldOfView = field; });
    }

    public void DecreaseFieldOfViewB()
    {
        LeanTween.value(80f, 60f, 1f).setEaseInOutQuart().setIgnoreTimeScale(true).setOnUpdate((float field) => { mainCameraB.fieldOfView = field; });
    }

    public void ShowInfo(string infoString)
    {
        showingInfo = true;
        infoText.text = infoString;
        infoDisappearTime = Time.unscaledTime + 3f;
        infoCanvas.LeanAlpha(1f, halfSecond).setEaseInOutQuart().setIgnoreTimeScale(true);
        if (delayedInfoAppear > 0f) { delayedInfoAppear -= 2f; }
    }

    public IEnumerator DelayedShowInfo(string infoString)
    {
        delayedInfoAppear += 2f;
        yield return new WaitForSecondsRealtime(delayedInfoAppear);
        ShowInfo(infoString);
    }

    public void ShowRaceOver()
    {
        cameraSwirl = true;
        humanCar.controlType = Player.ControlTypes.robot;
        FadeIn(overCanvas);
        StartCoroutine(FadeOut(countdownCanvas));
        StartCoroutine(FadeOut(raceCanvas));
        StartCoroutine(FadeOut(buttonsCanvas));
        StartCoroutine(FadeOut(timerCanvas));
        StartCoroutine(FadeOut(infoCanvas));
        StartCoroutine(ShowRaceResults());
        raceOver = true;
    }

    public IEnumerator ShowRaceOverA()
    {
        raceOver = true;
        cameraSwirl = true;
        animating = true;
        humanCar.controlType = Player.ControlTypes.robot;
        FadeIn(overCanvasA);
        StartCoroutine(FadeOut(countdownCanvas));
        StartCoroutine(FadeOut(raceCanvasA));
        bool showLeaderboard = raceOverB;
        if (showLeaderboard)
        {
            StartCoroutine(FadeOut(buttonsCanvas2P));
            StartCoroutine(FadeOut(timerCanvas));
            StartCoroutine(FadeOut(infoCanvas));
        }
        yield return new WaitForSecondsRealtime(2f);
        animating = false;
        if (showLeaderboard)
        {
            resultsText[6].text = carRank.ToString();
            StartCoroutine(ShowLeaderboardA());
        }
        else
        {
            resultsText[7].text = carRank.ToString();
            resultsText[8].text = "2 Players";
            resultsText[9].text = mapName.ToString();
            resultsText[10].text = raceMode.ToString();
            resultsText[11].text = numLaps.ToString();
            resultsText[12].text = difficultyType.ToString();
            FadeIn(resultsCanvasA);
        }
    }

    public IEnumerator ShowRaceOverB()
    {
        raceOverB = true;
        cameraSwirlB = true;
        animating = true;
        humanCarB.controlType = Player.ControlTypes.robot;
        FadeIn(overCanvasB);
        StartCoroutine(FadeOut(countdownCanvas));
        StartCoroutine(FadeOut(raceCanvasB));
        bool showLeaderboard = raceOver;
        if (showLeaderboard)
        {
            StartCoroutine(FadeOut(buttonsCanvas2P));
            StartCoroutine(FadeOut(timerCanvas));
            StartCoroutine(FadeOut(infoCanvas));
        }
        yield return new WaitForSecondsRealtime(2f);
        animating = false;
        if (showLeaderboard)
        {
            resultsText[13].text = carRankB.ToString();
            StartCoroutine(ShowLeaderboardB());
        }
        else
        {
            resultsText[14].text = carRankB.ToString();
            resultsText[15].text = "2 Players";
            resultsText[16].text = mapName.ToString();
            resultsText[17].text = raceMode.ToString();
            resultsText[18].text = numLaps.ToString();
            resultsText[19].text = difficultyType.ToString();
            FadeIn(resultsCanvasB);
        }
    }

    private IEnumerator ShowRaceResults()
    {
        animating = true;
        WaitForSecondsRealtime delay = new WaitForSecondsRealtime(0.1f);
        resultsText[0].text = carRank.ToString();
        if (humanCar.carEliminated)
        { resultsStars[0].SetActive(true); }
        else
        {   switch (humanCar.carRank)
            {
                case 1:
                    if ((gameType == Menu.GameTypes.play) && ((PlayerPrefs.GetInt("Stars" + selectedLevel.ToString(), 0)) < 3))
                    { PlayerPrefs.SetInt("Stars" + selectedLevel.ToString(), 3); }
                    resultsStars[3].SetActive(true);
                    break;
                case 2:
                    if ((gameType == Menu.GameTypes.play) && ((PlayerPrefs.GetInt("Stars" + selectedLevel.ToString(), 0)) < 2))
                    { PlayerPrefs.SetInt("Stars" + selectedLevel.ToString(), 2); }
                    resultsStars[2].SetActive(true);
                    break;
                case 3:
                    if ((gameType == Menu.GameTypes.play) && ((PlayerPrefs.GetInt("Stars" + selectedLevel.ToString(), 0)) < 1))
                    { PlayerPrefs.SetInt("Stars" + selectedLevel.ToString(), 1); }
                    resultsStars[1].SetActive(true);
                    break;
                default:
                    resultsStars[0].SetActive(true);
                    break;
            }
        }
        if (gameType == Menu.GameTypes.play)
        { resultsText[1].text = ("Level " + selectedLevel.ToString()); }
        else
        { resultsText[1].text = "1 Player"; }
        resultsText[2].text = mapName.ToString();
        resultsText[3].text = raceMode.ToString();
        resultsText[4].text = numLaps.ToString();
        resultsText[5].text = difficultyType.ToString();
        if ((gameType == Menu.GameTypes.play) && (resultsStars[3].activeSelf))
        {
            resultsText[20].text = string.Empty;
            switch (selectedLevel)
            {
                case 1:
                    if (PlayerPrefs.GetInt("Car2", 0) == 1) { break; }
                    PlayerPrefs.SetInt("Car2", 1);
                    resultsText[20].text = "NEW CAR UNLOCKED";
                    resultsText[21].text = GameManager.instance.cars[2].name;
                    break;
                case 2:
                    if (PlayerPrefs.GetInt("Level4", 0) == 1) { break; }
                    PlayerPrefs.SetInt("Level4", 1);
                    resultsText[20].text = "NEW LEVEL UNLOCKED";
                    resultsText[21].text = "LEVEL 4";
                    break;
                case 3:
                    if (PlayerPrefs.GetInt("Car3", 0) == 1) { break; }
                    PlayerPrefs.SetInt("Car3", 1);
                    resultsText[20].text = "NEW CAR UNLOCKED";
                    resultsText[21].text = GameManager.instance.cars[3].name;
                    break;
                case 4:
                    if (PlayerPrefs.GetInt("Level5", 0) == 1) { break; }
                    PlayerPrefs.SetInt("Level5", 1);
                    resultsText[20].text = "NEW LEVEL UNLOCKED";
                    resultsText[21].text = "LEVEL 5";
                    break;
                case 5:
                    if (PlayerPrefs.GetInt("Car4", 0) == 1) { break; }
                    PlayerPrefs.SetInt("Car4", 1);
                    resultsText[20].text = "NEW CAR UNLOCKED";
                    resultsText[21].text = GameManager.instance.cars[4].name;
                    break;
                default:
                    resultsText[20].text = string.Empty;
                    break;
            }
            if (resultsText[20].text != string.Empty)
            { unlockText.SetActive(true); }
        }
        yield return new WaitForSecondsRealtime(2f);
        UpdateLeaderboard(ref leaderboardName);
        FadeIn(resultsCanvas);
        animating = false;
        while (!(cars[cars.Count - 1].raceOver))
        { UpdateLeaderboard(ref leaderboardName); yield return delay; }
    }

    private void UpdateLeaderboard(ref TextMeshProUGUI name)
    {
        cars.Sort((carX, carY) => carX.carRank.CompareTo(carY.carRank));
        name.text = string.Empty;
        for (int i = 0; i < cars.Count; i++)
        {
            name.text += $"{cars[i].carRank}\t{cars[i].gameObject.name}";
            if ((gameType != Menu.GameTypes.quickRace2P) && (humanCar.gameObject.GetInstanceID() == cars[i].gameObject.GetInstanceID()))
            { name.text += " (You)"; }
            else if ((gameType == Menu.GameTypes.quickRace2P) && (humanCar.gameObject.GetInstanceID() == cars[i].gameObject.GetInstanceID()))
            { name.text += " (A)"; }
            else if ((gameType == Menu.GameTypes.quickRace2P) && (humanCarB.gameObject.GetInstanceID() == cars[i].gameObject.GetInstanceID()))
            { name.text += " (B)"; }
            if (cars[i].carEliminated)
            { name.text += " (Out)"; }
            if (i != cars.Count - 1) { name.text += System.Environment.NewLine; }
        }
    }

    private void UpdateLeaderboardTime(ref TextMeshProUGUI time)
    {
        time.text = string.Empty;
        for (int i = 0; i < cars.Count; i++)
        {
            if (cars[i].totalRaceTime > 599.99f) { cars[i].totalRaceTime = 599.99f; }
            time.text += $"{(int)(cars[i].totalRaceTime)/60}:{(cars[i].totalRaceTime)%60:00.00}";
            if (i != cars.Count - 1) { time.text += System.Environment.NewLine; }
        }
    }

    private IEnumerator ShowLeaderboardA()
    {
        animating = true;
        WaitForSecondsRealtime delay = new WaitForSecondsRealtime(0.1f);
        yield return new WaitForSecondsRealtime(2f);
        UpdateLeaderboard(ref leaderboardNameA);
        FadeIn(leaderboardCanvasA);
        animating = false;
        while (!(cars[cars.Count - 1].raceOver))
        { UpdateLeaderboard(ref leaderboardNameA); yield return delay; }
    }

    private IEnumerator ShowLeaderboardB()
    {
        animating = true;
        WaitForSecondsRealtime delay = new WaitForSecondsRealtime(0.1f);
        yield return new WaitForSecondsRealtime(2f);
        UpdateLeaderboard(ref leaderboardNameB);
        FadeIn(leaderboardCanvasB);
        animating = false;
        while (!(cars[cars.Count - 1].raceOver))
        { UpdateLeaderboard(ref leaderboardNameB); yield return delay; }
    }

    private IEnumerator RestartRace()
    {
        StartCoroutine(FadeOut(overCanvas));
        animating = true;
        FadeIn(loadingCanvas);
        music.audioSource.Stop();
        yield return new WaitForSecondsRealtime(2f);
        PlayerPrefs.SetFloat("Volume", Menu.ins.soundVolume);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        animating = false;
    }

    private IEnumerator BackToMenu()
    {
        Menu.showLoadingCanvas = true;
        StartCoroutine(FadeOut(overCanvas));
        animating = true;
        FadeIn(loadingCanvas);
        Menu.ins.raceSong = music.audioSource.clip;
        PlayerPrefs.SetFloat("Volume", Menu.ins.soundVolume);
        PlayerPrefs.Save();
        yield return new WaitForSecondsRealtime(0.6f);
        SceneManager.LoadScene(0);
        animating = false;
    }

    private void FadeIn(CanvasGroup canvasGroup)
    {
        canvasGroup.alpha = 0f;
        canvasGroup.gameObject.SetActive(true);
        canvasGroup.LeanAlpha(1f, halfSecond).setEaseInOutQuart().setIgnoreTimeScale(true);
    }

    private IEnumerator FadeOut(CanvasGroup canvasGroup)
    {
        animating = true;
        canvasGroup.LeanAlpha(0f, halfSecond).setEaseInOutQuart().setIgnoreTimeScale(true);
        yield return halfSecondDelay;
        canvasGroup.gameObject.SetActive(false);
        canvasGroup.alpha = 1f;
        animating = false;
    }

    public void ConfirmQuit()
    { FadeIn(quitCanvas); StartCoroutine(FadeOut(pauseCanvas)); windowOpen = true; }
    public void DeclineQuit()
    { FadeIn(pauseCanvas); StartCoroutine(FadeOut(quitCanvas)); windowOpen = false; }
    public void ConfirmRestart()
    { FadeIn(restartCanvas); StartCoroutine(FadeOut(pauseCanvas)); windowOpen = true; }
    public void DeclineRestart()
    { FadeIn(pauseCanvas); StartCoroutine(FadeOut(restartCanvas)); windowOpen = false; }
    public void BackToMenuWrapper() { animating = true; StartCoroutine(BackToMenu()); }
    public void RestartRaceWrapper() { animating = true; StartCoroutine(RestartRace()); }

}
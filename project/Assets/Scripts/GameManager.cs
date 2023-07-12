using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{

    public static GameManager instance;
    public DriftController driftController;
    public GameObject carsParent, startingPositionsParent;
    public InputController inputController;
    public InputControllerB inputControllerB;
    public int numCars, numLaps, selectedCar, selectedCarB, selectedLevel;
    public List<GameObject> cars;
    public Menu.DifficultyTypes difficultyType;
    public Menu.GameTypes gameType;
    public Menu.MapNames mapName;
    public Menu.RaceModes raceMode;
    public Slider volumeSlider;
    public Transform humanCar, humanCarB;
    public UIController UIController;
    private bool carPositionAssigned;
    private GameObject startingPositionsSubParent;
    private int randomCarIndex, selectedCarPosition, selectedCarPositionB, totalNumCars;
    private List<Transform> startingPositions;
    private System.Random randomNum;

    private void Awake()
    {
        instance = this;
        inputController = GetComponentInChildren<InputController>();
        UIController = GetComponentInChildren<UIController>();
        driftController = GetComponentInChildren<DriftController>();
        gameType = Menu.ins.gameType;
        raceMode = Menu.ins.raceMode;
        mapName = Menu.ins.mapName;
        difficultyType = Menu.ins.difficultyType;
        numLaps = Menu.ins.numLaps;
        numCars = Menu.ins.numCars;
        selectedLevel = Menu.ins.selectedLevel;
        selectedCar = Menu.ins.selectedCar;
        if (gameType == Menu.GameTypes.quickRace2P)
        {
            inputControllerB = GetComponentInChildren<InputControllerB>();
            UIController.mainCamera.gameObject.SetActive(true);
            UIController.mainCamera.rect = new Rect(0.5f, 0f, 0.5f, 1f);
            UIController.mainCameraB.gameObject.SetActive(true);
            UIController.mainCameraB.rect = new Rect(0f, 0f, 0.5f, 1f);
            selectedCarB = Menu.ins.selectedCarB;
        }
        else
        {
            UIController.mainCamera.gameObject.SetActive(true);
            UIController.mainCamera.rect = new Rect(0f, 0f, 1f, 1f);
        }
        totalNumCars = Menu.ins.totalNumCars;
        randomNum = new System.Random();
        cars = new List<GameObject>();
        startingPositions = new List<Transform>();
        startingPositionsSubParent = (startingPositionsParent.transform.GetChild(numCars - 1)).gameObject;
        foreach (Transform car in carsParent.transform)
        { cars.Add(car.gameObject); }
        foreach (Transform position in startingPositionsSubParent.transform)
        { startingPositions.Add(position); }
        for (int i = 0; i < totalNumCars; i++)
        { cars[i].SetActive(false); }
        AudioListener.volume = volumeSlider.value = Menu.ins.soundVolume;
        AssignCarPositions();
    }

    private void AssignCarPositions()
    {
        selectedCarPosition = randomNum.Next(0, numCars);
        cars[selectedCar].transform.position = startingPositions[selectedCarPosition].transform.position;
        cars[selectedCar].SetActive(true);
        cars[selectedCar].GetComponent<Player>().controlType = Player.ControlTypes.human;
        humanCar = cars[selectedCar].GetComponent<Transform>();
        if (gameType == Menu.GameTypes.quickRace2P)
        {
            selectedCarPositionB = randomNum.Next(0, numCars - 1);
            if (selectedCarPositionB >= selectedCarPosition)
            { selectedCarPositionB++; }
            cars[selectedCarB].transform.position = startingPositions[selectedCarPositionB].transform.position;
            cars[selectedCarB].SetActive(true);
            cars[selectedCarB].GetComponent<Player>().controlType = Player.ControlTypes.humanB;
            humanCarB = cars[selectedCarB].GetComponent<Transform>();
        }
        for (int i = 0; i < numCars; i++)
        {
            if ((i == selectedCarPosition) || ((i == selectedCarPositionB) && (gameType == Menu.GameTypes.quickRace2P)))
            { continue; }
            carPositionAssigned = false;
            while (!carPositionAssigned)
            {
                randomCarIndex = randomNum.Next(0, totalNumCars);
                if (cars[randomCarIndex].activeSelf) { continue; }
                cars[randomCarIndex].transform.position = startingPositions[i].transform.position;
                cars[randomCarIndex].SetActive(true);
                cars[randomCarIndex].GetComponent<Player>().controlType = Player.ControlTypes.robot;
                carPositionAssigned = true;
            }
        }
    }

}
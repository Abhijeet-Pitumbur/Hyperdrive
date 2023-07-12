using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{

    public AudioSource audioSource;
    public AudioClip impactSound;
    public ControlTypes controlType;
    public enum ControlTypes { human, humanB, robot, idle }
    public bool carEliminated, carInfected, raceOver;
    public float bestLapTime, currentLapTime, lastLapTime, totalRaceTime;
    public GameObject carsParent, waypointsParent;
    public int carRank, currentLap, numCars, waypointDistanceOffset;
    public List<Player> cars;
    public ParticleSystem infectionSmoke;
    public Rigidbody carBody;
    public Transform checkpointsParent, currentWaypoint;
    private bool recentReset;
    private CarController carController;
    private float currentWaypointDistance, lapTimeStamp, maxRobotThrottle, minRobotThrottle, maxRobotSteer, minRobotSteer, raceTimeStamp, robotSteer, robotThrottle, waypointDistance;
    private int carLayer, checkpointCount, checkpointLayer, propsLayer, lapProgess, previousCheckpoint, numLaps, waypointProgess;
    private List<Transform> waypoints;
    private Menu.DifficultyTypes difficultyType;
    private Menu.GameTypes gameType;
    private Menu.RaceModes raceMode;
    private string colliderName, carName;
    private TrackWaypoints trackWaypoints;
    private Transform resetCheckpoint;
    private Vector3 waypointDistanceDifference, waypointPosition, waypointRelativeDistance;
    private WaitForSeconds delay;

    private void Awake()
    {
        currentLap = 0; currentLapTime = 0; lastLapTime = 0; bestLapTime = Mathf.Infinity;
        checkpointCount = checkpointsParent.childCount;
        checkpointLayer = LayerMask.NameToLayer("Checkpoint");
        propsLayer = LayerMask.NameToLayer("Props");
        carLayer = LayerMask.NameToLayer("Car");
        carController = GetComponent<CarController>();
        carBody = GetComponent<Rigidbody>();
        trackWaypoints = waypointsParent.GetComponent<TrackWaypoints>();
        waypoints = trackWaypoints.waypoints;
        waypointDistanceOffset = 2; waypointProgess = 0; previousCheckpoint = 0;
        carEliminated = false; carInfected = false; raceOver = false; recentReset = false;
        infectionSmoke.Stop();
        audioSource = GetComponent<AudioSource>();
        delay = new WaitForSeconds(3f);
    }

    private void Start()
    {
        gameType = GameManager.instance.gameType;
        raceMode = GameManager.instance.raceMode;
        difficultyType = GameManager.instance.difficultyType;
        switch(difficultyType)
        {
            case Menu.DifficultyTypes.Easy: maxRobotThrottle = 0.7f; maxRobotSteer = 1.6f; break;
            case Menu.DifficultyTypes.Hard: maxRobotThrottle = 1.6f; maxRobotSteer = 2.5f; break;
            default: maxRobotThrottle = 1.2f; maxRobotSteer = 2.1f; break;
        }
        robotThrottle = maxRobotThrottle - 0.3f;
        robotSteer = maxRobotSteer - 0.2f;
        minRobotThrottle = 0.4f;
        minRobotSteer = 1.1f;
        numLaps = GameManager.instance.numLaps;
        numCars = GameManager.instance.numCars;
        raceTimeStamp = Time.time;
        foreach (Transform car in carsParent.transform)
        {
            Player player = car.GetComponent<Player>();
            if ((player != null) && (car.gameObject.activeSelf))
            { cars.Add(player); }
        }
        StartLap();
        StartCoroutine(IntervalUpdate());
        StartCoroutine(UpdateRank());
        if ((raceMode == Menu.RaceModes.Escape) || (raceMode == Menu.RaceModes.Survival) || (raceMode == Menu.RaceModes.Infection) || (raceMode == Menu.RaceModes.Special))
        { StartCoroutine(ModeTimer()); }
        carName = gameObject.name;
        if ((gameType != Menu.GameTypes.quickRace2P) && (controlType == Player.ControlTypes.human))
        { carName += " (You)"; }
        else if ((gameType == Menu.GameTypes.quickRace2P) && (controlType == Player.ControlTypes.human))
        { carName += " (A)"; }
        else if ((gameType == Menu.GameTypes.quickRace2P) && (controlType == Player.ControlTypes.humanB))
        { carName += " (B)"; }
    }

    private void StartLap()
    {
        currentLap++;
        previousCheckpoint = 1;
        lapTimeStamp = Time.time;
    }

    private void EndLap()
    {
        lastLapTime = Time.time - lapTimeStamp;
        if (lastLapTime > 599.99f) { lastLapTime = 599.99f; }
        bestLapTime = Mathf.Min(lastLapTime, bestLapTime);
        if (bestLapTime > 599.99f) { bestLapTime = 599.99f; }
        currentWaypoint = waypoints[0];
        waypointProgess = 0;
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.layer != checkpointLayer) { return; }
        colliderName = collider.gameObject.name;
        if (colliderName == "1")
        {
            if (previousCheckpoint == checkpointCount)
            { EndLap(); }
            if (currentLap == 0 || previousCheckpoint == checkpointCount)
            { StartLap(); }
            if (currentLap == numLaps + 1)
            { raceOver = true; }
            if ((raceOver) && (controlType == ControlTypes.human) && (gameType != Menu.GameTypes.quickRace2P))
            {
                GameManager.instance.UIController.ShowRaceOver();
                GameManager.instance.UIController.music.audioSource.PlayOneShot(GameManager.instance.UIController.finishSound);
                GameManager.instance.UIController.confetti.Play();
            }
            else if ((raceOver) && (controlType == ControlTypes.human) && (gameType == Menu.GameTypes.quickRace2P))
            {
                StartCoroutine(GameManager.instance.UIController.ShowRaceOverA());
                GameManager.instance.UIController.music.audioSource.PlayOneShot(GameManager.instance.UIController.finishSound);
                GameManager.instance.UIController.confetti.Play();
            }
            else if ((raceOver) && (controlType == ControlTypes.humanB) && (gameType == Menu.GameTypes.quickRace2P))
            {
                StartCoroutine(GameManager.instance.UIController.ShowRaceOverB());
                GameManager.instance.UIController.music.audioSource.PlayOneShot(GameManager.instance.UIController.finishSound);
                GameManager.instance.UIController.confettiB.Play();
            }
            return;
        }
        if (colliderName == (previousCheckpoint + 1).ToString())
        { previousCheckpoint++; }
    }

    private void Update()
    {
        UpdateWaypoint();
        if (controlType == ControlTypes.human) { HumanControl(); }
        else if (controlType == ControlTypes.humanB) { HumanControlB(); }
        else if (controlType == ControlTypes.robot) { RobotControl(); }
        if (currentLapTime < 599.99f)
        { currentLapTime = Time.time - lapTimeStamp; }
        else
        { currentLapTime = 599.99f; }
        if ((!raceOver) && (totalRaceTime < 599.99f))
        { totalRaceTime = Time.time - raceTimeStamp; }
        if (Input.GetKeyDown(KeyCode.Return) && (controlType == Player.ControlTypes.human) && (!GameManager.instance.UIController.racePaused) && (totalRaceTime > 3f) && (!recentReset))
        { StartCoroutine(ResetCarPosition()); recentReset = true; StartCoroutine(UpdateRecentReset()); }
        else if (Input.GetKeyDown(KeyCode.Tab) && (controlType == Player.ControlTypes.humanB) && (!GameManager.instance.UIController.racePaused) && (totalRaceTime > 3f) && (!recentReset))
        { StartCoroutine(ResetCarPosition()); recentReset = true; StartCoroutine(UpdateRecentReset()); }
    }

    private void HumanControl()
    {
        carController.steer = GameManager.instance.inputController.steerInput;
        carController.throttle = GameManager.instance.inputController.throttleInput;
    }

    private void HumanControlB()
    {
        carController.steer = GameManager.instance.inputControllerB.steerInputB;
        carController.throttle = GameManager.instance.inputControllerB.throttleInputB;
    }

    private void RobotControl()
    {
        waypointRelativeDistance = transform.InverseTransformPoint(currentWaypoint.position);
        waypointRelativeDistance /= waypointRelativeDistance.magnitude;
        carController.steer = (waypointRelativeDistance.x / waypointRelativeDistance.magnitude) * robotSteer;
        carController.throttle = robotThrottle;
    }

    private void UpdateWaypoint()
    {
        waypointPosition = transform.position;
        waypointDistance = Mathf.Infinity;
        for (int i = 0; i < waypoints.Count; i++)
        {
            waypointDistanceDifference = waypoints[i].position - waypointPosition;
            currentWaypointDistance = waypointDistanceDifference.magnitude;
            if ((currentWaypointDistance < waypointDistance) && (i >= waypointProgess) && (i <= waypointProgess + 5) && (currentWaypoint != waypoints[waypoints.Count - 1]))
            {
                currentWaypoint = waypoints[i + waypointDistanceOffset];
                waypointDistance = currentWaypointDistance;
                waypointProgess = i;
            }
        }
    }

    private IEnumerator UpdateRank()
    {
        WaitForSeconds delay = new WaitForSeconds(0.1f);
        int tempCarRank;
        int otherCarCurrentLap;
        int thisCarCurrentWaypoint;
        int otherCarCurrentWaypoint;
        float thisCarCurrentWaypointDistance;
        float otherCarCurrentWaypointDistance;
        carRank = 1;
        while (!raceOver)
        {
            yield return delay;
            tempCarRank = 1;
            for (int i = 0; i < cars.Count; i++)
            {
                if ((gameObject.GetInstanceID() == cars[i].gameObject.GetInstanceID()) || (cars[i].carEliminated))
                { continue; }
                otherCarCurrentLap = cars[i].currentLap;
                if (currentLap < otherCarCurrentLap) { tempCarRank++; }
                else if (currentLap == otherCarCurrentLap)
                {
                    thisCarCurrentWaypoint = int.Parse(currentWaypoint.gameObject.name);
                    otherCarCurrentWaypoint = int.Parse(cars[i].currentWaypoint.gameObject.name);
                    if (thisCarCurrentWaypoint < otherCarCurrentWaypoint) { tempCarRank++; }
                    else if (thisCarCurrentWaypoint == otherCarCurrentWaypoint)
                    {
                        thisCarCurrentWaypointDistance = (currentWaypoint.position - transform.position).sqrMagnitude;
                        otherCarCurrentWaypointDistance = (cars[i].currentWaypoint.position - cars[i].transform.position).sqrMagnitude;
                        if (thisCarCurrentWaypointDistance > otherCarCurrentWaypointDistance) { tempCarRank++; }
                    }
                }
            }
            carRank = tempCarRank;
        }
    }

    private IEnumerator ModeTimer()
    {
        int timerTime;
        int timerTimeLeft;
        WaitForSeconds oneSecond = new WaitForSeconds(1f);
        timerTime = 45;
        yield return new WaitForSeconds(0.1f);
        while (!raceOver)
        {
            timerTimeLeft = timerTime;
            lapProgess = currentLap;
            while (timerTimeLeft > 0)
            {
                if (timerTimeLeft >= 10)
                { GameManager.instance.UIController.timerText.text = "0:" + timerTimeLeft.ToString(); }
                else
                { GameManager.instance.UIController.timerText.text = "0:0" + timerTimeLeft.ToString(); }
                if ((timerTimeLeft == 3) && (controlType == ControlTypes.human))
                { GameManager.instance.UIController.music.audioSource.PlayOneShot(GameManager.instance.UIController.timerSound); }
                yield return oneSecond;
                timerTimeLeft--;
            }
            if (raceOver) { break; }
            StartCoroutine(ModeTimerOver());
            if (timerTime > 25) { timerTime -= 10; }
        }
    }

    private IEnumerator ModeTimerOver()
    {
        switch (raceMode)
        {
            case Menu.RaceModes.Escape:
            {
                if (lapProgess == currentLap)
                { EliminateCar(); }
                break;
            }
            case Menu.RaceModes.Survival:
            {
                if (carRank == numCars)
                { EliminateCar(); }
                break;
            }
            case Menu.RaceModes.Infection:
            {
                if (carRank == numCars)
                { StartCoroutine(InfectCar()); }
                break;
            }
            case Menu.RaceModes.Special:
            {
                if ((lapProgess == currentLap) || (carRank == numCars))
                { EliminateCar(); }
                break;
            }
        }
        yield return new WaitForSeconds(0.5f);
        numCars = GameManager.instance.UIController.numCars;
    }

    private void EliminateCar()
    {
        GameManager.instance.UIController.numCars--;
        string infoString;
        raceOver = true;
        carEliminated = true;
        if (controlType == Player.ControlTypes.robot)
        { StartCoroutine(EliminateRobot()); }
        else if ((controlType == Player.ControlTypes.human) && (gameType != Menu.GameTypes.quickRace2P))
        {
            GameManager.instance.UIController.music.audioSource.PlayOneShot(GameManager.instance.UIController.eliminateSound);
            GameManager.instance.UIController.ShowRaceOver();
        }
        else if ((controlType == Player.ControlTypes.human) && (gameType == Menu.GameTypes.quickRace2P))
        {
            GameManager.instance.UIController.music.audioSource.PlayOneShot(GameManager.instance.UIController.eliminateSound);
            StartCoroutine(GameManager.instance.UIController.ShowRaceOverA());
        }
        else if ((controlType == Player.ControlTypes.humanB) && (gameType == Menu.GameTypes.quickRace2P))
        {
            GameManager.instance.UIController.music.audioSource.PlayOneShot(GameManager.instance.UIController.eliminateSound);
            StartCoroutine(GameManager.instance.UIController.ShowRaceOverB());
        }
        infoString = carName + " ELIMINATED";
        if (GameManager.instance.UIController.showingInfo)
        { StartCoroutine(GameManager.instance.UIController.DelayedShowInfo(infoString)); }
        else
        { GameManager.instance.UIController.ShowInfo(infoString); }
    }

    private IEnumerator EliminateRobot()
    {
        controlType = Player.ControlTypes.idle;
        carController.steer = 0f;
        carController.throttle = 0f;
        yield return new WaitForSeconds(10f);
        gameObject.SetActive(false);
    }

    private IEnumerator InfectCar()
    {
        string infoString;
        carInfected = true;
        carController.maxThrottle += 1500f;
        infectionSmoke.Play();
        if (controlType == Player.ControlTypes.human)
        { 
            GameManager.instance.UIController.music.audioSource.PlayOneShot(GameManager.instance.UIController.infectSound);
            GameManager.instance.UIController.IncreaseFieldOfView();
        }
        else if (controlType == Player.ControlTypes.humanB)
        {
            GameManager.instance.UIController.music.audioSource.PlayOneShot(GameManager.instance.UIController.infectSound);
            GameManager.instance.UIController.IncreaseFieldOfViewB();
        }
        infoString = carName + " INFECTED";
        if (GameManager.instance.UIController.showingInfo)
        { StartCoroutine(GameManager.instance.UIController.DelayedShowInfo(infoString)); }
        else
        { GameManager.instance.UIController.ShowInfo(infoString); }
        yield return new WaitForSeconds(10f);
        DisinfectCar();
    }

    private void DisinfectCar()
    {
        string infoString;
        carController.maxThrottle -= 1500f;
        infectionSmoke.Stop();
        if (gameObject.GetInstanceID() == GameManager.instance.UIController.humanCar.gameObject.GetInstanceID())
        {
            GameManager.instance.UIController.music.audioSource.PlayOneShot(GameManager.instance.UIController.disinfectSound);
            GameManager.instance.UIController.DecreaseFieldOfView();
        }
        else if ((gameType == Menu.GameTypes.quickRace2P) && (gameObject.GetInstanceID() == GameManager.instance.UIController.humanCarB.gameObject.GetInstanceID()))
        { 
            GameManager.instance.UIController.music.audioSource.PlayOneShot(GameManager.instance.UIController.disinfectSound);
            GameManager.instance.UIController.DecreaseFieldOfViewB();
        }
        infoString = carName + " DISINFECTED";
        if (GameManager.instance.UIController.showingInfo)
        { StartCoroutine(GameManager.instance.UIController.DelayedShowInfo(infoString)); }
        else
        { GameManager.instance.UIController.ShowInfo(infoString); }
        carInfected = false;
    }

    private void OnCollisionEnter(Collision collider)
    {
        if ((collider.gameObject.layer == carLayer) || (collider.gameObject.layer == propsLayer))
        { audioSource.PlayOneShot(impactSound, 1f); }
        if ((raceMode != Menu.RaceModes.Infection) || (collider.gameObject.layer != carLayer) || (raceOver) || (carInfected))
        { return; }
        Player otherCar = collider.gameObject.GetComponent<Player>();
        if (otherCar.carInfected) { StartCoroutine(InfectCar()); }
    }

    private IEnumerator IntervalUpdate()
    {
        while (true)
        {
            yield return delay;
            if (transform.position.y <= -20f) { StartCoroutine(ResetCarPosition()); }
            if ((controlType == ControlTypes.robot) && ((carBody.velocity.sqrMagnitude < 4f) || (currentWaypoint == waypoints[waypoints.Count - 1])))
            { StartCoroutine(CheckStuckRobot()); }
            robotThrottle = Random.Range(minRobotThrottle, maxRobotThrottle);
            robotSteer = Random.Range(minRobotSteer, maxRobotSteer);
        }
    }

    private IEnumerator CheckStuckRobot()
    {
        yield return delay;
        if ((carBody.velocity.sqrMagnitude < 4f) || (currentWaypoint == waypoints[waypoints.Count - 1]))
        { StartCoroutine(ResetCarPosition()); }
    }

    private IEnumerator ResetCarPosition()
    {
        GetResetPosition();
        if (controlType == ControlTypes.robot)
        { yield return new WaitForSeconds(Random.Range(0, 3)); }
        transform.position = resetCheckpoint.position;
        transform.rotation = resetCheckpoint.rotation;
        carBody.velocity = Vector3.zero;
        carBody.angularVelocity = Vector3.zero;
    }

    private void GetResetPosition()
    {
        if ((controlType == ControlTypes.human) || (controlType == ControlTypes.humanB))
        {
            GameManager.instance.UIController.music.audioSource.PlayOneShot(GameManager.instance.UIController.disinfectSound);
            resetCheckpoint = checkpointsParent.Find(previousCheckpoint.ToString()); return;
        }
        if (previousCheckpoint != checkpointCount)
        { resetCheckpoint = checkpointsParent.Find((previousCheckpoint + 1).ToString()); return; }
        resetCheckpoint = checkpointsParent.Find("1");
    }

    private IEnumerator UpdateRecentReset()
    { yield return delay; recentReset = false; }

    private void OnDrawGizmos()
    { Gizmos.DrawWireSphere(currentWaypoint.position, 2f); }

}
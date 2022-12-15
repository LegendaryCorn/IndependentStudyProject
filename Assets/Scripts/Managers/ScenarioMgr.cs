using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenarioMgr : MonoBehaviour
{
    public static ScenarioMgr instance;

    Scenario currScenario;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    void GenerateScenario()
    {
        var lane1Nodes = AIMgr.instance.GetNodesInArea("Lane1");
        var lane2Nodes = AIMgr.instance.GetNodesInArea("Lane2");
        var zone1Nodes = AIMgr.instance.GetNodesInArea("Zone1");
        var zone2Nodes = AIMgr.instance.GetNodesInArea("Zone2");

        var s = new Scenario();
        s.scenarioShips = new List<ScenarioShip>();

        // Lane 1 Ships (10)
        for(int i = 0; i < 10; i++)
        {
            var newShip = new ScenarioShip();
            newShip.startPoint = lane1Nodes[Random.Range(0, lane1Nodes.Count)].position;
            newShip.endPoint = zone2Nodes[Random.Range(0, zone2Nodes.Count)].position;

            newShip.minSpeed = 0;
            newShip.maxSpeed = Random.Range(4.0f, 12.0f);
            newShip.angularVelocity = Random.Range(0.05f, 0.15f);
            newShip.acceleration = Random.Range(2.0f, 4.0f);

            newShip.startHeading = 0;
            newShip.startSpeed = newShip.maxSpeed;
            s.scenarioShips.Add(newShip);
        }

        // Lane 2 Ships (10)
        for (int i = 0; i < 10; i++)
        {
            var newShip = new ScenarioShip();
            newShip.startPoint = lane2Nodes[Random.Range(0, lane2Nodes.Count)].position;
            newShip.endPoint = zone1Nodes[Random.Range(0, zone1Nodes.Count)].position;

            newShip.minSpeed = 0;
            newShip.maxSpeed = Random.Range(4.0f, 12.0f);
            newShip.angularVelocity = Random.Range(0.05f, 0.15f);
            newShip.acceleration = Random.Range(2.0f, 4.0f);

            newShip.startHeading = Mathf.PI;
            newShip.startSpeed = newShip.maxSpeed;
            s.scenarioShips.Add(newShip);
        }

        // Zone 1 Ships (6)
        for (int i = 0; i < 6; i++)
        {
            var newShip = new ScenarioShip();
            newShip.startPoint = zone1Nodes[Random.Range(0, zone1Nodes.Count)].position;
            newShip.endPoint = zone2Nodes[Random.Range(0, zone2Nodes.Count)].position;

            newShip.minSpeed = 0;
            newShip.maxSpeed = Random.Range(4.0f, 12.0f);
            newShip.angularVelocity = Random.Range(0.05f, 0.15f);
            newShip.acceleration = Random.Range(2.0f, 4.0f);

            newShip.startHeading = 0;
            newShip.startSpeed = newShip.maxSpeed;
            s.scenarioShips.Add(newShip);
        }

        // Zone 2 Ships (6)
        for (int i = 0; i < 6; i++)
        {
            var newShip = new ScenarioShip();
            newShip.startPoint = zone2Nodes[Random.Range(0, zone2Nodes.Count)].position;
            newShip.endPoint = zone1Nodes[Random.Range(0, zone1Nodes.Count)].position;

            newShip.minSpeed = 0;
            newShip.maxSpeed = Random.Range(4.0f, 12.0f);
            newShip.angularVelocity = Random.Range(0.05f, 0.15f);
            newShip.acceleration = Random.Range(2.0f, 4.0f);

            newShip.startHeading = Mathf.PI;
            newShip.startSpeed = newShip.maxSpeed;
            s.scenarioShips.Add(newShip);
        }

        currScenario = s;
    }

    void LoadScenario()
    {
        foreach(ScenarioShip ship in currScenario.scenarioShips)
        {
            ShipMgr.instance.SpawnScenarioShip(ship);
        }
    }

    void Start()
    {
        GenerateScenario();
        LoadScenario();
    }


    void Update()
    {

    }
    
}

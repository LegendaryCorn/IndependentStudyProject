using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenarioMgr : MonoBehaviour
{
    public static ScenarioMgr instance;

    [SerializeField] private List<Scenario> scenarios;
    [SerializeField] private int currScenario;

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

    void LoadScenario(int s)
    {
        var sce = scenarios[s];

        //SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        foreach(ScenarioShip ship in sce.scenarioShips)
        {
            ShipMgr.instance.SpawnScenarioShip(ship);
        }
        currScenario = s;
    }

    void Start()
    {
        LoadScenario(0);
    }


    void Update()
    {

    }
    
}

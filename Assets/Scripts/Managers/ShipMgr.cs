using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipMgr : MonoBehaviour
{
    public static ShipMgr instance;

    public GameObject shipObject;

    public Dictionary<int, Ship> shipDict;

    public Dictionary<int, Dictionary<int, CPA>> cpaDict;
    public Dictionary<int, Dictionary<int, CPA>> prevCpaDict;

    [SerializeField] private float updateSeconds;
    [SerializeField] private float updateSecondsFluctuation;

    public int shipIDSet = 0;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }

        shipDict = new Dictionary<int, Ship>();
        cpaDict = new Dictionary<int, Dictionary<int, CPA>>();
        prevCpaDict = new Dictionary<int, Dictionary<int, CPA>>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var newCpaDict = new Dictionary<int, Dictionary<int, CPA>>();
        foreach (int s1 in shipDict.Keys)
        {
            newCpaDict[s1] = new Dictionary<int, CPA>();
            foreach (int s2 in shipDict.Keys)
            {
                if(cpaDict.ContainsKey(s1) && cpaDict[s1].ContainsKey(s2) && cpaDict[s1][s2].refreshTime > Time.realtimeSinceStartup)
                {
                    newCpaDict[s1][s2] = cpaDict[s1][s2];
                }
                else
                {
                    newCpaDict[s1][s2] = new CPA(shipDict[s1], shipDict[s2]);
                }
            }
        }

        prevCpaDict = cpaDict;
        cpaDict = newCpaDict;
    }

    public void SpawnNewShip(int teamID, Vector3 spawnPos)
    {
        GameObject newShip = Instantiate(shipObject);
        Ship newPlayerShip = newShip.GetComponent<Ship>();
        newPlayerShip.shipTeam = teamID;
        newPlayerShip.transform.position = spawnPos;
        newPlayerShip.physics.SetPosition(spawnPos);
        newPlayerShip.shipID = shipIDSet;
        shipDict[shipIDSet] = newShip.GetComponent<Ship>();
        shipIDSet++;
    }

    public void SpawnScenarioShip(ScenarioShip scenarioShip)
    {
        GameObject newShip = Instantiate(shipObject);
        Ship newPlayerShip = newShip.GetComponent<Ship>();
        newPlayerShip.shipTeam = 0;
        newPlayerShip.transform.position = scenarioShip.startPoint.transform.position;
        newPlayerShip.physics.SetScenarioShip(scenarioShip);
        for(int i = 0; i < scenarioShip.waypoints.Count; i++)
        {
            newPlayerShip.ai.AddDesiredPosition(scenarioShip.waypoints[i].transform.position);
        }
        newPlayerShip.shipID = shipIDSet;
        shipDict[shipIDSet] = newShip.GetComponent<Ship>();
        shipIDSet++;
    }

    public struct CPA
    {
        public float cpaDist { get; private set; } // The closest distance between the two ships
        public float cpaTime { get; private set; } // The exact time that this will occur
        public float refreshTime { get; private set; } // The time to refresh this

        public CPA(Ship yourShip, Ship otherShip)
        {
            // Get position variables as we use these a lot
            var yourShipPos = yourShip.transform.position;
            var otherShipPos = otherShip.transform.position;

            // Calculate relative position and velocity
            var relPos = otherShipPos - yourShipPos;
            var bearing = Mathf.Rad2Deg * Mathf.Atan2(relPos.z, relPos.x);
            var relVel = otherShip.physics.GetVelocity() - yourShip.physics.GetVelocity();

            var relDist = relPos.magnitude;
            var relSpeed = relVel.magnitude;

            // CPA
            var angleBeta = Mathf.Acos(Vector3.Dot(relVel, -relPos) / (relSpeed * relDist));
            float DCPA = relDist * Mathf.Sin(angleBeta);
            float TCPA = relDist * Mathf.Cos(angleBeta) / relSpeed;

            // CBDR
            var prevRelPos = otherShip.ai.prevPos - yourShip.ai.prevPos;
            var prevDist = prevRelPos.magnitude;
            var prevBearing = Mathf.Rad2Deg * Mathf.Atan2(prevRelPos.z, prevRelPos.x);
            var bearingDelta = Mathf.Abs(Mathf.DeltaAngle(prevBearing, bearing)) / Time.deltaTime;

            // Put it in the variables
            cpaDist = DCPA;
            cpaTime = Time.realtimeSinceStartup + TCPA;
            refreshTime = Time.realtimeSinceStartup + ShipMgr.instance.updateSeconds + Random.Range(-ShipMgr.instance.updateSecondsFluctuation, ShipMgr.instance.updateSecondsFluctuation);
        }

        public float CalcTCPA()
        {
            return cpaTime - Time.realtimeSinceStartup;
        }
    }
}

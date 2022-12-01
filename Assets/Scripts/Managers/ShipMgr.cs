using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipMgr : MonoBehaviour
{
    public static ShipMgr instance;

    public GameObject shipObject;

    public Dictionary<int, Ship> shipDict;


    int shipIDSet = 0;

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
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
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
}

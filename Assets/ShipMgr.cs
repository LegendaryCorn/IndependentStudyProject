using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ShipMgr : MonoBehaviour
{
    public static ShipMgr instance;

    public GameObject shipObject;

    public Dictionary<int, PlayerShip> shipDict;

    public List<Material> teamMaterials;

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

        shipDict = new Dictionary<int, PlayerShip>();
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
        PlayerShip newPlayerShip = newShip.GetComponent<PlayerShip>();
        newPlayerShip.SetTeam(teamID);
        newPlayerShip.Teleport(spawnPos);
        newPlayerShip._shipID.Value = shipIDSet;
        shipDict[shipIDSet] = newShip.GetComponent<PlayerShip>();
        shipIDSet++;
        newShip.GetComponent<NetworkObject>().Spawn();
    }
}

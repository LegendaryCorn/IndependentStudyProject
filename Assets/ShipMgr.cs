using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ShipMgr : MonoBehaviour
{
    public static ShipMgr instance;

    public GameObject shipObject;

    public List<PlayerShip> shipList;

    public List<Material> teamMaterials;

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
        shipList.Add(newShip.GetComponent<PlayerShip>());
        newShip.GetComponent<NetworkObject>().Spawn();

    }
}

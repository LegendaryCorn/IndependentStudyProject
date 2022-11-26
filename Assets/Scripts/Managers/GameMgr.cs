using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Unity.Netcode;

public class GameMgr : NetworkBehaviour
{
    public Dictionary<ulong, Ship> shipDict;

    public static GameMgr instance;

    public ulong userID;
    public int teamID;

    public GameObject pref;

    private void Awake()
    {
        if(instance != null)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }

        shipDict = new Dictionary<ulong, Ship>();
        userID = 98989898989;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsHost)
        {
            AIMgr.instance.GenerateField();
            AIMgr.instance.ShowField();
        }
    }

    void Start()
    {
        
    }


    void Update()
    {
        
    }
}

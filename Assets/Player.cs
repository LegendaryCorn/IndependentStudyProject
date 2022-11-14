using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Player : NetworkBehaviour
{
    private NetworkVariable<int> _teamID = new NetworkVariable<int>(writePerm: NetworkVariableWritePermission.Owner);

    public int teamID;
    public string username;

    bool initialized = false;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            GameMgr.instance.userID = OwnerClientId;
            _teamID.Value = GameMgr.instance.teamID;
        }

        base.OnNetworkSpawn();
        EventMgr.instance.onPlayerJoin.Invoke();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}

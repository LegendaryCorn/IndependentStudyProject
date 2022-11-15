using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Player : NetworkBehaviour
{
    private NetworkVariable<int> _teamID = new NetworkVariable<int>(writePerm: NetworkVariableWritePermission.Owner);
    private NetworkVariable<bool> _hasSpawned = new NetworkVariable<bool>(writePerm: NetworkVariableWritePermission.Owner);

    public NetworkVariable<Vector3> _mousePlanePos = new NetworkVariable<Vector3>(writePerm: NetworkVariableWritePermission.Owner, readPerm: NetworkVariableReadPermission.Owner);
    public NetworkVariable<bool> _spawnRequest = new NetworkVariable<bool>(writePerm: NetworkVariableWritePermission.Owner, readPerm: NetworkVariableReadPermission.Owner);

    public int teamID;
    public string username;
    private bool hasSpawned;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsOwner)
        {
            PlayerMgr.instance.userID = OwnerClientId;
            _teamID.Value = PlayerMgr.instance.teamID;
            _hasSpawned.Value = true;
        }

        PlayerMgr.instance.playerDict[OwnerClientId] = this;
        EventMgr.instance.onPlayerJoin.Invoke();
    }

    public override void OnNetworkDespawn()
    {
        PlayerMgr.instance.playerDict.Remove(OwnerClientId);
        base.OnNetworkDespawn();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(!hasSpawned && _hasSpawned.Value)
        {
            teamID = _teamID.Value;
            hasSpawned = true;
        }
        if (IsServer)
        {
            HandleRequests();
        }
    }

    private void FixedUpdate()
    {

    }


    bool spawnReqPrev = false;

    void HandleRequests()
    {
        if (_spawnRequest.Value && !spawnReqPrev)
        {
            ShipMgr.instance.SpawnNewShip(teamID, _mousePlanePos.Value);
        }
        spawnReqPrev = _spawnRequest.Value;
    }
}

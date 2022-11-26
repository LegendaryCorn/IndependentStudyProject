using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Player : NetworkBehaviour
{
    public NetworkVariable<int> teamID = new NetworkVariable<int>(writePerm: NetworkVariableWritePermission.Owner);

    public PlayerControl control;
    public PlayerSelection selection;

    private void Awake()
    {

    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsOwner)
        {
            Destroy(selection);
            selection = null;
        }

        if (IsOwner)
        {
            PlayerMgr.instance.userID = OwnerClientId;
            teamID.Value = PlayerMgr.instance.teamID;
        }

        PlayerMgr.instance.playerDict[OwnerClientId] = this;
        EventMgr.instance.onPlayerJoin.Invoke();
    }

    public override void OnNetworkDespawn()
    {
        PlayerMgr.instance.playerDict.Remove(OwnerClientId);
        base.OnNetworkDespawn();
    }
}

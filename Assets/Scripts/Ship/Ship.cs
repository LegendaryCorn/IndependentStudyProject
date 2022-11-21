using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Ship : NetworkBehaviour
{
    public ShipPhysics physics;
    public ShipAI ai;

    public NetworkVariable<int> shipTeam = new NetworkVariable<int>(writePerm: NetworkVariableWritePermission.Server);
    public NetworkVariable<int> shipID = new NetworkVariable<int>(writePerm: NetworkVariableWritePermission.Server);

    private readonly Color[] teamColors = { Color.blue, Color.red, Color.gray };

    public MeshRenderer shipMarker;
    public GameObject friendlyMarker;
    public GameObject enemyMarker;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsServer)
        {
            Destroy(ai);
            ai = null;
            ShipSetup();
        }
        else
        {
            physics.SetPosition(transform.position);
            ShipSetup();
            ShipSetupClientRpc();
        }
    }

    [ClientRpc]
    private void ShipSetupClientRpc()
    {
        ShipSetup();
    } 

    private void ShipSetup()
    {
        ShipMgr.instance.shipDict[shipID.Value] = this;
        shipMarker.material.color = teamColors[shipTeam.Value];
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
    }


    void Start()
    {

    }

    private void Update()
    {

    }


    void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;
    }
}

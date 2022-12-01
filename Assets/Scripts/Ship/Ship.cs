using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ship : MonoBehaviour
{
    public ShipPhysics physics;
    public ShipAI ai;

    public int shipTeam;
    public int shipID;

    private readonly Color[] teamColors = { Color.blue, Color.red, Color.gray };

    public MeshRenderer shipMarker;
    public GameObject friendlyMarker;
    public GameObject enemyMarker;

    private void Awake()
    {
        physics.SetPosition(transform.position);
        shipMarker.material.color = teamColors[shipTeam];
    }

    void OnDestroy()
    {
        ShipMgr.instance.shipDict.Remove(shipID);
    }
}

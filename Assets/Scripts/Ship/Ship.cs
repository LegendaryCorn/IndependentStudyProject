using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ship : MonoBehaviour
{
    public ShipPhysics physics;
    public ShipAI ai;
    public ShipRender render;

    public int shipTeam;
    public int shipID;

    private void Awake()
    {
        physics.SetPosition(transform.position);
    }

    void OnDestroy()
    {
        ShipMgr.instance.shipDict.Remove(shipID);
    }
}

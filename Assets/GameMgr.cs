using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Unity.Netcode;

public class GameMgr : NetworkBehaviour
{
    public Dictionary<ulong, PlayerShip> shipDict;

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

        shipDict = new Dictionary<ulong, PlayerShip>();
        userID = 98989898989;
    }

    void Start()
    {
        
    }


    void Update()
    {
        
    }

    public void ChangeDesiredSpeed(float newSpeed)
    {
        shipDict[userID].desiredSpeed = Mathf.Clamp(newSpeed, shipDict[userID].minSpeed, shipDict[userID].maxSpeed);
        EventMgr.instance.onDesiredSpeedChanged.Invoke();
    }

    public void ChangeDesiredHeading(float newHeading)
    {
        shipDict[userID].desiredHeading = newHeading * Mathf.Deg2Rad;
        if (shipDict[userID].desiredHeading < 0) shipDict[userID].desiredHeading += 2 * Mathf.PI;
        if (shipDict[userID].desiredHeading >= 2 * Mathf.PI) shipDict[userID].desiredHeading -= 2 * Mathf.PI;
        EventMgr.instance.onDesiredHeadingChanged.Invoke();
    }
}

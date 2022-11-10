using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ControlMgr : NetworkBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float dt = Time.deltaTime;
        DoInputs(dt);
    }

    void DoInputs(float dt)
    {
        if (GameMgr.instance.shipDict.ContainsKey(GameMgr.instance.userID))
        {
            PlayerShip pShip = GameMgr.instance.shipDict[GameMgr.instance.userID];
            if (Input.GetAxisRaw("Vertical") != 0)
            {
                pShip.desiredSpeed = Mathf.Clamp(pShip.desiredSpeed + dt * pShip.desiredSpeedChangeRate * Input.GetAxisRaw("Vertical"), pShip.minSpeed, pShip.maxSpeed);
                EventMgr.instance.onDesiredSpeedChanged.Invoke();
            }
            if (Input.GetAxisRaw("Horizontal") != 0)
            {
                pShip.desiredHeading += dt * pShip.desiredHeadingChangeRate * Input.GetAxisRaw("Horizontal");
                if (pShip.desiredHeading < 0) pShip.desiredHeading += 2 * Mathf.PI;
                if (pShip.desiredHeading >= 2 * Mathf.PI) pShip.desiredHeading -= 2 * Mathf.PI;
                EventMgr.instance.onDesiredHeadingChanged.Invoke();
            }
            if (Input.GetAxisRaw("Jump") != 0)
            {
                pShip.desiredSpeed = 0;
                EventMgr.instance.onDesiredSpeedChanged.Invoke();
            }
        }
    }
}

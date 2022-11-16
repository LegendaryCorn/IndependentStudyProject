using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ControlMgr : NetworkBehaviour
{
    public float camMoveAdjustRate;
    public float camRotAdjustRate;
    public float camZoomAdjustRate;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float dt = Time.deltaTime;
        if (PlayerMgr.instance.CheckIfOnline())
        {
            DoPlayerInputs(dt);
            DoCameraInputs(dt);
        }
    }

    void DoPlayerInputs(float dt)
    {
        Player yourPlayer = PlayerMgr.instance.GetClientPlayer();

        yourPlayer._spawnRequest.Value = (Input.GetAxisRaw("Spawn") != 0);
        yourPlayer._selectRequest.Value = (Input.GetAxisRaw("Selection") != 0);
        yourPlayer._moveRequest.Value = (Input.GetAxisRaw("Movement") != 0);


        Vector3 mouseToWater = Vector3.zero;
        Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
        yourPlayer._mouseRay.Value = r;

        /*
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
        */
    }

    void DoCameraInputs(float dt)
    {
        if(Input.GetAxisRaw("CamVertical") != 0 || Input.GetAxisRaw("CamHorizontal") != 0)
        {
            if(Input.GetAxisRaw("CamRotate") != 0)
            {
                Vector2 inp = new Vector2(Input.GetAxisRaw("CamVertical"), Input.GetAxisRaw("CamHorizontal"));
                Vector2 adjustment = dt * camRotAdjustRate * (inp.normalized);
                CameraMgr.instance.AdjustRotation(adjustment);
            }
            else
            {
                Vector3 inp = new Vector3(Input.GetAxisRaw("CamHorizontal"), 0, Input.GetAxisRaw("CamVertical"));
                Vector3 adjustment = dt * camMoveAdjustRate * (inp.normalized);
                CameraMgr.instance.PanCamera(adjustment);
            }
        }
        if (Input.GetAxisRaw("Mouse ScrollWheel") != 0)
        {
            float adjustment = -camZoomAdjustRate * Input.GetAxisRaw("Mouse ScrollWheel");
            CameraMgr.instance.AdjustZoom(adjustment);
        }

    }
}

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
        yourPlayer._lShiftRequest.Value = (Input.GetAxisRaw("LShift") != 0);

        Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
        yourPlayer._mouseRay.Value = r;
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

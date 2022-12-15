using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    private Player player;
    private Ship followShip = null;

    [SerializeField] private float camMoveAdjustRate;
    [SerializeField] private float camRotAdjustRate;
    [SerializeField] private float camZoomAdjustRate;

    private void Awake()
    {
        player = gameObject.GetComponent<Player>();
    }



    void Update()
    {
        DoFollowShipInput();
        DoShipInputs(Time.deltaTime);
        DoCameraInputs(Time.deltaTime);
    }

    #region ShipInputs

    bool prevFollowPress = false;
    bool prevSpawnPress = false;
    bool prevSelectPress = false;
    bool prevMovePress = false;

    void DoFollowShipInput()
    {
        var currFollowPress = Input.GetAxisRaw("Follow") != 0;

        if(currFollowPress && !prevFollowPress)
        {
            if(followShip == null && player.selection.selectedShipList.Count == 1)
            {
                followShip = player.selection.selectedShipList[0];
                CameraMgr.instance.controlled = true;
            }
            else if(followShip != null)
            {
                followShip = null;
                CameraMgr.instance.controlled = false;
            }
        }

        prevFollowPress = currFollowPress;
    }

    void DoShipInputs(float dt)
    {
        if (followShip == null)
        {
            var currSpawnPress = (Input.GetAxisRaw("Spawn") != 0);
            var currSelectPress = (Input.GetAxisRaw("Selection") != 0);
            var currMovePress = (Input.GetAxisRaw("Movement") != 0);

            var lshift = (Input.GetAxisRaw("LShift") != 0);

            if (currSpawnPress && !prevSpawnPress) SpawnShip();
            if (currSelectPress && !prevSelectPress) SelectShip(lshift);
            if (currMovePress && !prevMovePress) MoveOrder(lshift);

            prevSpawnPress = currSpawnPress;
            prevSelectPress = currSelectPress;
            prevMovePress = currMovePress;
        }
        else
        {
            if (Input.GetAxisRaw("DirectControlHeading") != 0 || Input.GetAxisRaw("DirectControlSpeed") != 0)
            {
                followShip.physics.DirectControl(dt, new Vector2(Input.GetAxisRaw("DirectControlHeading"), Input.GetAxisRaw("DirectControlSpeed")));
            }
        }
    }

    void SpawnShip()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        LayerMask waterMask = LayerMask.GetMask("Water");
        LayerMask terrainMask = LayerMask.GetMask("Terrain");
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, waterMask) && !Physics.Raycast(ray, out _, Mathf.Infinity, terrainMask))
        {
            ShipMgr.instance.SpawnNewShip(player.teamID, hit.point);
        }
    }

    void SelectShip(bool lshift)
    {
        if (!lshift) player.selection.ClearList();

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        LayerMask shipMask = LayerMask.GetMask("Ship");
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, shipMask))
        {
            Ship selectedShip = hit.collider.gameObject.GetComponent<Ship>();
            player.selection.AddShipToList(selectedShip);
        }
    }

    void MoveOrder(bool lshift)
    {
        var movePoint = Vector3.zero;
        var pointHit = false;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        LayerMask waterMask = LayerMask.GetMask("Water");
        LayerMask terrainMask = LayerMask.GetMask("Terrain");
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, waterMask) && !Physics.Raycast(ray, out _, Mathf.Infinity, terrainMask))
        {
            movePoint = hit.point;
            pointHit = true;
        }

        foreach (Ship s in player.selection.selectedShipList)
        {
            if (s.shipTeam == player.teamID)
            {
                MoveShip(lshift, s.shipID, movePoint, pointHit);
            }
        }
    }

    void MoveShip(bool lshift, int shipID, Vector3 movePos, bool move)
    {
        Ship s = ShipMgr.instance.shipDict[shipID];

        if (!lshift)
        {
            s.ai.ClearDesiredPositions();
        }

        if (move)
        {
            s.ai.AddDesiredPosition(movePos);
        }
    }

    #endregion

    void DoCameraInputs(float dt)
    {
        if (followShip == null)
        {
            if (Input.GetAxisRaw("CamVertical") != 0 || Input.GetAxisRaw("CamHorizontal") != 0 || Input.GetAxisRaw("CamUpDown") != 0)
            {
                Vector3 inp = new Vector3(Input.GetAxisRaw("CamHorizontal"), Input.GetAxisRaw("CamUpDown"), Input.GetAxisRaw("CamVertical"));
                Vector3 adjustment = dt * camMoveAdjustRate * (inp.normalized) * (Input.GetAxisRaw("PanFast") * 9 + 1);
                CameraMgr.instance.PanCamera(adjustment);
            }
            if (Input.GetAxisRaw("CamRotateUpDown") != 0 || Input.GetAxisRaw("CamRotateLeftRight") != 0)
            {
                Vector2 inp = new Vector2(Input.GetAxisRaw("CamRotateUpDown"), Input.GetAxisRaw("CamRotateLeftRight"));
                Vector2 adjustment = dt * camRotAdjustRate * (inp.normalized);
                CameraMgr.instance.AdjustRotation(adjustment);
            }
        }
        else
        {
            Camera.main.transform.position = followShip.transform.position + new Vector3(30 * Mathf.Sin(followShip.transform.eulerAngles.y * Mathf.Deg2Rad), 15, 30 * Mathf.Cos(followShip.transform.eulerAngles.y * Mathf.Deg2Rad));
            Camera.main.transform.eulerAngles = followShip.transform.eulerAngles;
        }
    }
}

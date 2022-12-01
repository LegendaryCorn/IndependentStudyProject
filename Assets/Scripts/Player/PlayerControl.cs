using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    private Player player;

    [SerializeField] private float camMoveAdjustRate;
    [SerializeField] private float camRotAdjustRate;
    [SerializeField] private float camZoomAdjustRate;

    private void Awake()
    {
        player = gameObject.GetComponent<Player>();
    }



    void Update()
    {
        DoShipInputs(Time.deltaTime);
        DoCameraInputs(Time.deltaTime);
    }

    #region ShipInputs

    bool prevSpawnPress = false;
    bool prevSelectPress = false;
    bool prevMovePress = false;

    void DoShipInputs(float dt)
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
        if (Input.GetAxisRaw("CamVertical") != 0 || Input.GetAxisRaw("CamHorizontal") != 0)
        {
            if (Input.GetAxisRaw("CamRotate") != 0)
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

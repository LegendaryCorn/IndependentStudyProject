using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Player : NetworkBehaviour
{
    private NetworkVariable<int> _teamID = new NetworkVariable<int>(writePerm: NetworkVariableWritePermission.Owner);
    private NetworkVariable<bool> _hasSpawned = new NetworkVariable<bool>(writePerm: NetworkVariableWritePermission.Owner);

    public NetworkVariable<bool> _spawnRequest = new NetworkVariable<bool>(writePerm: NetworkVariableWritePermission.Owner, readPerm: NetworkVariableReadPermission.Owner);
    public NetworkVariable<bool> _selectRequest = new NetworkVariable<bool>(writePerm: NetworkVariableWritePermission.Owner, readPerm: NetworkVariableReadPermission.Owner);
    public NetworkVariable<bool> _moveRequest = new NetworkVariable<bool>(writePerm: NetworkVariableWritePermission.Owner, readPerm: NetworkVariableReadPermission.Owner);
    public NetworkVariable<bool> _lShiftRequest = new NetworkVariable<bool>(writePerm: NetworkVariableWritePermission.Owner, readPerm: NetworkVariableReadPermission.Owner);

    public NetworkVariable<Ray> _mouseRay = new NetworkVariable<Ray>(writePerm: NetworkVariableWritePermission.Owner, readPerm: NetworkVariableReadPermission.Owner);

    public NetworkList<int> _selectedShipList;

    public int teamID;
    public string username;
    private bool hasSpawned;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsOwner)
        {
            PlayerMgr.instance.userID = OwnerClientId;
            _teamID.Value = PlayerMgr.instance.teamID;
            _hasSpawned.Value = true;
        }
        
        PlayerMgr.instance.playerDict[OwnerClientId] = this;
        EventMgr.instance.onPlayerJoin.Invoke();
    }

    public override void OnNetworkDespawn()
    {
        PlayerMgr.instance.playerDict.Remove(OwnerClientId);
        base.OnNetworkDespawn();
    }

    // Start is called before the first frame update
    void Awake()
    {
        _selectedShipList = new NetworkList<int>(writePerm: NetworkVariableWritePermission.Server, readPerm: NetworkVariableReadPermission.Owner);
    }

    // Update is called once per frame
    void Update()
    {
        if(!hasSpawned && _hasSpawned.Value)
        {
            teamID = _teamID.Value;
            hasSpawned = true;
        }
        if (IsServer)
        {
            HandleRequests();
        }
        if (IsOwner)
        {
            UpdateShipIndicators();
        }
    }

    private void FixedUpdate()
    {

    }


    bool spawnReqPrev = false;
    bool selectReqPrev = false;
    bool moveReqPrev = false;

    void HandleRequests()
    {

        LayerMask waterMask = LayerMask.GetMask("Water");
        RaycastHit hit;
        Vector3 mouseToWater = Vector3.zero;
        if (Physics.Raycast(_mouseRay.Value, out hit, Mathf.Infinity, waterMask))
        {
            mouseToWater = hit.point;
        }

        LayerMask shipMask = LayerMask.GetMask("Ship");
        Ship mouseToShip = null;
        if (Physics.Raycast(_mouseRay.Value, out hit, Mathf.Infinity, shipMask))
        {
            mouseToShip = hit.collider.gameObject.GetComponent<Ship>();
        }

        if (_spawnRequest.Value && !spawnReqPrev && !mouseToWater.Equals(Vector3.zero))
        {
            ShipMgr.instance.SpawnNewShip(teamID, mouseToWater);
        }
        spawnReqPrev = _spawnRequest.Value;

        if(_selectRequest.Value && !selectReqPrev)
        {
            if (!_lShiftRequest.Value)
            {
                _selectedShipList.Clear();
            }

            if (mouseToShip != null)
            {
                _selectedShipList.Add(mouseToShip.shipID.Value);

            }
        }
        selectReqPrev = _selectRequest.Value;

        if(_moveRequest.Value && !moveReqPrev)
        {
            if(!mouseToWater.Equals(Vector3.zero))
            {
                foreach(int s in _selectedShipList)
                {
                    Ship ship = ShipMgr.instance.shipDict[s];
                    if (ship.shipTeam.Value == teamID)
                    {
                        if (!_lShiftRequest.Value)
                        {
                            ship.ai.ClearDesiredPositions();
                        }
                        ship.ai.AddDesiredPosition(mouseToWater);
                    }
                }
            }
        }
        moveReqPrev = _moveRequest.Value;
        
    }

    void UpdateShipIndicators()
    {
        foreach (int i in ShipMgr.instance.shipDict.Keys)
        {
            Ship currShip = ShipMgr.instance.shipDict[i];
            if (_selectedShipList.Contains(currShip.shipID.Value))
            {
                if (currShip.shipTeam.Value == _teamID.Value)
                {
                    currShip.friendlyMarker.SetActive(true);
                }
                else
                {
                    currShip.enemyMarker.SetActive(true);
                }
            }
            else
            {
                currShip.friendlyMarker.SetActive(false);
                currShip.enemyMarker.SetActive(false);
            }
        }
    }
}

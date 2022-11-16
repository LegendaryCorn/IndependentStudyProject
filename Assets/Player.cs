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

    public int teamID;
    public string username;
    private bool hasSpawned;

    private List<PlayerShip> selectedShipList = new List<PlayerShip>();

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
    void Start()
    {

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
        PlayerShip mouseToShip = null;
        if (Physics.Raycast(_mouseRay.Value, out hit, Mathf.Infinity, shipMask))
        {
            mouseToShip = hit.collider.gameObject.GetComponent<PlayerShip>();
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
                selectedShipList.Clear();
            }

            if (mouseToShip != null)
            {
                selectedShipList.Add(mouseToShip);

            }
        }
        selectReqPrev = _selectRequest.Value;

        if(_moveRequest.Value && !moveReqPrev)
        {
            if(!mouseToWater.Equals(Vector3.zero))
            {
                foreach(PlayerShip ship in selectedShipList)
                {
                    if (ship._shipTeam.Value == teamID)
                    {
                        if (!_lShiftRequest.Value)
                        {
                            ship.desiredPositionList.Clear();
                        }
                        ship.desiredPositionList.Add(mouseToWater);
                        //ship.controlledPlayer = PlayerMgr.instance.playerDict.ge
                    }
                }
            }
        }
        moveReqPrev = _moveRequest.Value;
        
    }
}

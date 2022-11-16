using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerShip : NetworkBehaviour
{

    private NetworkVariable<float> _desiredSpeed = new NetworkVariable<float>(writePerm: NetworkVariableWritePermission.Server);
    private NetworkVariable<float> _desiredHeading = new NetworkVariable<float>(writePerm: NetworkVariableWritePermission.Server);

    private NetworkVariable<Vector3> _position = new NetworkVariable<Vector3>(writePerm: NetworkVariableWritePermission.Server);
    private NetworkVariable<float> _speed = new NetworkVariable<float>(writePerm: NetworkVariableWritePermission.Server);
    private NetworkVariable<float> _heading = new NetworkVariable<float>(writePerm: NetworkVariableWritePermission.Server);

    public NetworkVariable<int> _shipTeam = new NetworkVariable<int>(writePerm: NetworkVariableWritePermission.Server);

    public float desiredSpeed;
    public float desiredHeading;

    public float desiredSpeedChangeRate;
    public float desiredHeadingChangeRate;

    public Vector3 position;
    public float speed;
    public float acceleration;
    public float minSpeed;
    public float maxSpeed;

    public float heading;
    public float angularVelocity;

    public float minMagnitude;
    public ulong controlledPlayer;
    public List<Vector3> desiredPositionList;

    public MeshRenderer shipMarker;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsServer)
        {
            ShipMgr.instance.shipList.Add(this);
            shipMarker.material = ShipMgr.instance.teamMaterials[_shipTeam.Value];
        }
    }

    public override void OnNetworkDespawn()
    {
        //GameMgr.instance.shipDict.Remove(OwnerClientId);
        //EventMgr.instance.onPlayerLeave.Invoke();
        base.OnNetworkDespawn();
    }


    void Start()
    {

    }

    private void Update()
    {
        float dt = Time.deltaTime;
        DoPhysics(dt);
    }

    void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;

        CheckDesiredPosition(dt);
        CalcDesiredSpeedHeading(dt);
        UpdatePositions(dt);
    }

    void CheckDesiredPosition(float dt)
    {
        if(desiredPositionList.Count > 0 && Vector3.SqrMagnitude(position - desiredPositionList[0]) < minMagnitude * minMagnitude)
        {
            desiredPositionList.RemoveAt(0);
        }
    }

    void CalcDesiredSpeedHeading(float dt)
    {
        if (desiredPositionList.Count > 0)
        {
            desiredSpeed = maxSpeed;
            Vector3 posDiff = desiredPositionList[0] - position;
            desiredHeading = Mathf.Atan2(posDiff.x, posDiff.z);
            desiredHeading += desiredHeading < 0 ? 2 * Mathf.PI : 0;
        }
        else
        {
            desiredSpeed = 0;
        }
    }

    void DoPhysics(float dt)
    {
        if ((desiredHeading > heading && desiredHeading < heading + Mathf.PI) || desiredHeading < heading - Mathf.PI)
        {
            float realDesiredHeading = (desiredHeading < heading ? desiredHeading + 2 * Mathf.PI : desiredHeading);
            heading = Mathf.Min(heading + dt * angularVelocity, realDesiredHeading);
        }
        else
        {
            float realDesiredHeading = (desiredHeading > heading ? desiredHeading - 2 * Mathf.PI : desiredHeading);
            heading = Mathf.Max(heading - dt * angularVelocity, realDesiredHeading);
        }
        if (heading < 0) heading += 2 * Mathf.PI;
        if (heading >= 2 * Mathf.PI) heading -= 2 * Mathf.PI;

        if (speed < desiredSpeed)
        {
            speed = Mathf.Min(speed + acceleration * dt, desiredSpeed, maxSpeed);
        }
        else
        {
            speed = Mathf.Max(speed - acceleration * dt, desiredSpeed, minSpeed);
        }

        position += speed * dt * new Vector3(Mathf.Sin(heading), 0, Mathf.Cos(heading));
        gameObject.transform.position = position;
        gameObject.transform.eulerAngles = new Vector3(0, Mathf.Rad2Deg * heading, 0);
    }

    void UpdatePositions(float dt)
    {
        if (IsOwner)
        {
            _desiredSpeed.Value = desiredSpeed;
            _desiredHeading.Value = desiredHeading;
        }
        else
        {
            desiredSpeed = _desiredSpeed.Value;
            desiredHeading = _desiredHeading.Value;
        }

        if (IsServer)
        {
            _position.Value = position;
            _speed.Value = speed;
            _heading.Value = heading;
        }
        else
        {
            position = _position.Value;
            speed = _speed.Value;
            heading = _heading.Value;
        }
    }

    public void SetTeam(int t)
    {
        _shipTeam.Value = t;
        shipMarker.material = ShipMgr.instance.teamMaterials[t];
    }

    public void Teleport(Vector3 newPos)
    {
        _position.Value = newPos;
        transform.position = newPos;
        position = newPos;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerShip : NetworkBehaviour
{

    private NetworkVariable<float> _desiredSpeed = new NetworkVariable<float>(writePerm: NetworkVariableWritePermission.Owner);
    private NetworkVariable<float> _desiredHeading = new NetworkVariable<float>(writePerm: NetworkVariableWritePermission.Owner);

    private NetworkVariable<Vector3> _position = new NetworkVariable<Vector3>(writePerm: NetworkVariableWritePermission.Server);
    private NetworkVariable<float> _speed = new NetworkVariable<float>(writePerm: NetworkVariableWritePermission.Server);
    private NetworkVariable<float> _heading = new NetworkVariable<float>(writePerm: NetworkVariableWritePermission.Server);

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

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        GameMgr.instance.shipDict[OwnerClientId] = this;
        EventMgr.instance.onPlayerJoin.Invoke();

        if (IsOwner)
        {
            GameMgr.instance.userID = OwnerClientId;
        }
    }

    public override void OnNetworkDespawn()
    {
        GameMgr.instance.shipDict.Remove(OwnerClientId);
        EventMgr.instance.onPlayerLeave.Invoke();
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
        UpdatePositions(dt);
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
}

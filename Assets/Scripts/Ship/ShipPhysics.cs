using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ShipPhysics : NetworkBehaviour
{
    private Ship ship;

    public Vector3 desiredPosition;
    public bool hasDesiredPosition = false;
    private float desiredSpeed;
    private float desiredHeading;

    private NetworkVariable<ShipPhysicsNetworkData> physicsData;

    [SerializeField] private float acceleration;
    [SerializeField] private float minSpeed;
    [SerializeField] private float maxSpeed;
    [SerializeField] private float angularVelocity;

    private void Awake()
    {
        ship = gameObject.GetComponent<Ship>();
        physicsData = new NetworkVariable<ShipPhysicsNetworkData>(writePerm: NetworkVariableWritePermission.Server);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
    }

    void CalcDesiredSpeedHeading(float dt)
    {
        if (hasDesiredPosition)
        {
            desiredSpeed = maxSpeed;
            Vector3 posDiff = desiredPosition - physicsData.Value.Position;
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
        var currData = physicsData.Value;
        var newData = new ShipPhysicsNetworkData();


        if ((desiredHeading > currData.heading && desiredHeading < currData.heading + Mathf.PI) || desiredHeading < currData.heading - Mathf.PI)
        {
            float realDesiredHeading = (desiredHeading < currData.heading ? desiredHeading + 2 * Mathf.PI : desiredHeading);
            newData.heading = Mathf.Min(currData.heading + dt * angularVelocity, realDesiredHeading);
        }
        else
        {
            float realDesiredHeading = (desiredHeading > currData.heading ? desiredHeading - 2 * Mathf.PI : desiredHeading);
            newData.heading = Mathf.Max(currData.heading - dt * angularVelocity, realDesiredHeading);
        }
        if (newData.heading < 0) newData.heading += 2 * Mathf.PI;
        if (newData.heading >= 2 * Mathf.PI) newData.heading -= 2 * Mathf.PI;

        if (currData.speed < desiredSpeed)
        {
            newData.speed = Mathf.Min(currData.speed + acceleration * dt, desiredSpeed, maxSpeed);
        }
        else
        {
            newData.speed = Mathf.Max(currData.speed - acceleration * dt, desiredSpeed, minSpeed);
        }

        newData.Position = currData.Position + newData.speed * dt * new Vector3(Mathf.Sin(newData.heading), 0, Mathf.Cos(newData.heading));
        gameObject.transform.position = newData.Position;
        gameObject.transform.eulerAngles = new Vector3(0, Mathf.Rad2Deg * newData.heading, 0);

        physicsData.Value = newData;
    }


    void UpdatePositions(float dt)
    {
        if (IsServer)
        {
            CalcDesiredSpeedHeading(dt);
            DoPhysics(dt);
        }
        else
        {
            transform.position = physicsData.Value.Position;
            transform.eulerAngles = new Vector3(0, Mathf.Rad2Deg * physicsData.Value.heading, 0);
        }
    }

    public void SetPosition(Vector3 v)
    {
        physicsData.Value = new ShipPhysicsNetworkData
        {
            Position = v,
            speed = physicsData.Value.speed,
            heading = physicsData.Value.heading
        };
    }

    void Update()
    {
        UpdatePositions(Time.deltaTime);
    }

    struct ShipPhysicsNetworkData : INetworkSerializable
    {
        private float x, z;

        internal Vector3 Position
        {
            get => new Vector3(x, 0, z);
            set
            {
                x = value.x;
                z = value.z;
            }
        }

        internal float speed;
        internal float heading;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref x);
            serializer.SerializeValue(ref z);

            serializer.SerializeValue(ref speed);
            serializer.SerializeValue(ref heading);
        }
    }

}

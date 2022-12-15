using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipPhysics : MonoBehaviour
{
    private Ship ship;

    public float desiredSpeed;
    public float desiredHeading;

    private ShipPhysicsData physicsData;

    [SerializeField] private float acceleration;
    [SerializeField] private float minSpeed;
    public float maxSpeed;
    [SerializeField] private float angularVelocity;

    private void Awake()
    {
        ship = gameObject.GetComponent<Ship>();
        physicsData = new ShipPhysicsData();
    }


    void DoPhysics(float dt)
    {
        var currData = physicsData;
        var newData = new ShipPhysicsData();


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

        physicsData = newData;
    }

    public void DirectControl(float dt, Vector2 inputs)
    {
        ship.ai.ClearDesiredPositions();

        var newHeading = physicsData.heading + inputs.x * angularVelocity * dt;
        newHeading += (newHeading < 0 ? 2 * Mathf.PI : 0) - (newHeading > 2 * Mathf.PI ? 2 * Mathf.PI : 0);

        var newSpeed = physicsData.speed + inputs.y * acceleration * dt;
        newSpeed = Mathf.Clamp(newSpeed, minSpeed, maxSpeed);

        physicsData = new ShipPhysicsData()
        {
            Position = physicsData.Position,
            speed = newSpeed,
            heading = newHeading
        };

        desiredSpeed = physicsData.speed;
        desiredHeading = physicsData.heading;
    }

    void Update()
    {
        DoPhysics(Time.deltaTime);
    }

    #region Setters
    public void SetPosition(Vector3 v)
    {
        physicsData = new ShipPhysicsData()
        {
            Position = v,
            speed = physicsData.speed,
            heading = physicsData.heading
        };
    }

    public void SetDesiredSpeed(float s)
    {
        desiredSpeed = Mathf.Clamp(s, minSpeed, maxSpeed);
    }

    public void SetDesiredHeading(float h)
    {
        desiredHeading = (h < 0 ? 2 * Mathf.PI : 0) - (h > 2 * Mathf.PI ? 2 * Mathf.PI : 0) + h;
    }

    public void SetScenarioShip(ScenarioShip s)
    {
        physicsData = new ShipPhysicsData()
        {
            Position = s.startPoint,
            speed = s.startSpeed,
            heading = s.startHeading
        };

        minSpeed = s.minSpeed;
        maxSpeed = s.maxSpeed;
        acceleration = s.acceleration;
        angularVelocity = s.angularVelocity;
    }
    #endregion

    #region Getters
    public Vector3 GetVelocity()
    {
        return physicsData.speed * new Vector3(Mathf.Sin(physicsData.heading), 0, Mathf.Cos(physicsData.heading));
    }

    public float GetSpeed()
    {
        return physicsData.speed;
    }

    #endregion

    struct ShipPhysicsData
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
    }

}

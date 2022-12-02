using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipAI : MonoBehaviour
{
    private Ship ship;

    public float minMagnitude;

    [SerializeField] private float minCollisionDist;

    public List<Vector3> desiredPositionList;

    // Potential field at final location
    [SerializeField] private float positionPFConstant;
    [SerializeField] private float positionPFExponent;

    // Potential field to guide ship correctly
    private bool avoidancePFEnabled = false;
    private Vector3 avoidancePFLocation;
    [SerializeField] private float avoidancePFConstant;
    [SerializeField] private float avoidancePFExponent;

    private void Awake()
    {
        ship = gameObject.GetComponent<Ship>();
    }

    public void AddDesiredPosition(Vector3 pos)
    {
        Vector3 startPos;
        if(desiredPositionList.Count == 0)
        {
            startPos = gameObject.transform.position;
        }
        else
        {
            startPos = desiredPositionList[desiredPositionList.Count - 1];
        }
        var newDesiredPositions = AIMgr.instance.GeneratePath(startPos, pos);
        desiredPositionList.AddRange(newDesiredPositions);

        for(int i = 0; i < desiredPositionList.Count - 1; i++)
        {
            Debug.DrawLine(desiredPositionList[i], desiredPositionList[i + 1], Color.blue, 10);
        }
    }

    public void ClearDesiredPositions()
    {
        desiredPositionList.Clear();
    }

    private void Update()
    {
        CheckDesiredPosition(Time.deltaTime);
        CheckForCollisions(Time.deltaTime);
        CalcDesiredSpeedHeading(Time.deltaTime);
    }

    void CheckDesiredPosition(float dt)
    {
        if (desiredPositionList.Count > 0 && Vector3.SqrMagnitude(transform.position - desiredPositionList[0]) < minMagnitude * minMagnitude)
        {
            desiredPositionList.RemoveAt(0);
        }
    }

    void CheckForCollisions(float dt)
    {
        Ship closestShip = null;
        float closestDist = minCollisionDist;

        Vector3 yourShipPos = gameObject.transform.position;

        foreach(Ship otherShip in ShipMgr.instance.shipDict.Values)
        {
            var otherShipPos = otherShip.transform.position;

            // Calculate relative velocity
            var relVel = otherShip.physics.GetVelocity() - ship.physics.GetVelocity();

            // If heading towards, calculate minimum distance between two ships
            if(Vector3.Dot(relVel, otherShipPos - yourShipPos) < 0)
            {
                var a = 1.0f / relVel.x;
                var b = -1.0f / relVel.z;
                var c = (otherShipPos.z / relVel.z) - (otherShipPos.x / relVel.x);

                var dist = Mathf.Abs(a * yourShipPos.x + b * yourShipPos.z + c) / Mathf.Sqrt(a * a + b * b);

                // Record dist if close
                if(dist < closestDist)
                {
                    closestShip = otherShip;
                    closestDist = dist;
                }
            }
        }

        avoidancePFEnabled = false;

        // If there exists a closest ship, calculate angles and take appropriate action
        if (closestShip != null)
        {
            float angDiff = Mathf.DeltaAngle(gameObject.transform.eulerAngles.y, closestShip.transform.eulerAngles.y);
            float speedDiff = ship.physics.GetSpeed() - closestShip.physics.GetSpeed();
            string s = "";

            if (Mathf.Abs(angDiff) > 160)
            {
                s = "HeadOn";
                Vector3 normalizedVel = Vector3.Normalize(closestShip.physics.GetVelocity());
                avoidancePFLocation = closestShip.transform.position + 8 * new Vector3(-normalizedVel.z, 0, normalizedVel.x);
                avoidancePFEnabled = true;
            }
            else if(Mathf.Abs(angDiff) < 5 && speedDiff > 0)
            {
                s = "Overtaking";
            }
            else if (Mathf.Abs(angDiff) < 5 && speedDiff < 0)
            {
                s = "Overtaken";
            }
            else if (angDiff < 0)
            {
                s = "Crossing Turn to Starboard";
            }
            else
            {
                s = "Crossing Hold Course";
            }
            //Debug.Log(ship.shipID + " " + s);
        }
    }

    void CalcDesiredSpeedHeading(float dt)
    {
        if (desiredPositionList.Count > 0)
        {
            float dist = Vector3.Magnitude(gameObject.transform.position - desiredPositionList[0]);
            Vector3 totalForce = positionPFConstant * Mathf.Pow(dist, positionPFExponent) * Vector3.Normalize(desiredPositionList[0] - gameObject.transform.position);

            if (avoidancePFEnabled)
            {
                var avDist = Vector3.Magnitude(gameObject.transform.position - avoidancePFLocation);
                totalForce += avoidancePFConstant * Mathf.Pow(avDist, avoidancePFExponent) * Vector3.Normalize(avoidancePFLocation - gameObject.transform.position);
            }

            for(int i = 0; i < AIMgr.instance.potentialFields.Count; i++)
            {
                PotentialField p = AIMgr.instance.potentialFields[i];
                totalForce += p.CalcForce(ship);

            }
            Vector3 normalizedTotalForce = totalForce.normalized;
            var dh = Mathf.Atan2(normalizedTotalForce.x, normalizedTotalForce.z);

            ship.physics.SetDesiredSpeed(ship.physics.maxSpeed * (Mathf.Cos(Mathf.Deg2Rad * Mathf.DeltaAngle(dh * Mathf.Rad2Deg, transform.eulerAngles.y)) + 1) / 2);
            ship.physics.SetDesiredHeading(dh);
        }
        else
        {
            ship.physics.SetDesiredSpeed(0);
        }
    }

}

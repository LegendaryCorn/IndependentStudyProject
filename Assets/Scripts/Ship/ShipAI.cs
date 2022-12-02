using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipAI : MonoBehaviour
{
    private Ship ship;

    public float minMagnitude;

    [SerializeField] private float minCollisionDist;
    [SerializeField] private float maxCollisionDist;

    public List<Vector3> desiredPositionList;

    // Potential field at final location
    [SerializeField] private float positionPFConstant;
    [SerializeField] private float positionPFExponent;

    // Potential field to guide ship correctly
    private List<Vector3> avoidancePFList = new List<Vector3>();
    [SerializeField] private float avoidancePFConstant;
    [SerializeField] private float avoidancePFExponent;

    private void Awake()
    {
        ship = gameObject.GetComponent<Ship>();
    }

    public void AddDesiredPosition(Vector3 pos)
    {
        Vector3 startPos;
        if (desiredPositionList.Count == 0)
        {
            startPos = gameObject.transform.position;
        }
        else
        {
            startPos = desiredPositionList[desiredPositionList.Count - 1];
        }
        var newDesiredPositions = AIMgr.instance.GeneratePath(startPos, pos);
        desiredPositionList.AddRange(newDesiredPositions);

        for (int i = 0; i < desiredPositionList.Count - 1; i++)
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

    List<Ship> riskOfCollisionList = new List<Ship>();
    List<RiskTypes> riskTypeList = new List<RiskTypes>();
    enum RiskTypes {HeadOn, Overtaking, Overtaken, CrossingTurn, CrossingHold }
    void CheckForCollisions(float dt)
    {
        // Check for ships where there is a risk of collision
        // If there is a risk of collision, create a potential field
        // If there is no longer a risk of collision, remove that potential field

        // Check if the ships have been removed
        foreach(Ship s in riskOfCollisionList)
        {
            if (!ShipMgr.instance.shipDict.ContainsValue(s))
            {
                int i = riskOfCollisionList.IndexOf(s);
                riskOfCollisionList.RemoveAt(i);
                riskTypeList.RemoveAt(i);
                print(i);
            }
        }

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
                if(dist < minCollisionDist && !riskOfCollisionList.Contains(otherShip))
                {
                    riskOfCollisionList.Add(otherShip);
                    riskTypeList.Add(CalcRiskType(otherShip));
                }

                // Remove dist if far
                if (dist > maxCollisionDist && riskOfCollisionList.Contains(otherShip))
                {
                    int i = riskOfCollisionList.IndexOf(otherShip);
                    riskOfCollisionList.RemoveAt(i);
                    riskTypeList.RemoveAt(i);
                }
            }

            // If moving away, remove it from the list, as they will never collide
            else
            {
                if (riskOfCollisionList.Contains(otherShip))
                {
                    int i = riskOfCollisionList.IndexOf(otherShip);
                    riskOfCollisionList.RemoveAt(i);
                    riskTypeList.RemoveAt(i);
                }
            }
        }

        // Create potential fields
        avoidancePFList.Clear();

        for(int i = 0; i < riskOfCollisionList.Count; i++)
        {
            Ship s = riskOfCollisionList[i];
            Vector3 normalizedVel = Vector3.Normalize(s.physics.GetVelocity());

            switch (riskTypeList[i])
            {
                case RiskTypes.HeadOn:
                    print("Headon");
                    var h = s.transform.position + 15 * new Vector3(-0.0f * normalizedVel.x - normalizedVel.z, 0, normalizedVel.x - 0.0f * normalizedVel.z).normalized;
                    avoidancePFList.Add(h);
                    break;

                case RiskTypes.Overtaking:
                    var o = s.transform.position + 30 * new Vector3(0.0f * normalizedVel.x + normalizedVel.z, 0, -normalizedVel.x + 0.0f * normalizedVel.z).normalized;
                    avoidancePFList.Add(o);
                    break;

                case RiskTypes.CrossingTurn:
                    var c = s.transform.position + 40 * new Vector3(-0.0f * normalizedVel.z - normalizedVel.x, 0, -normalizedVel.z - 0.0f * normalizedVel.x).normalized;
                    avoidancePFList.Add(c);
                    break;

                case RiskTypes.Overtaken:
                case RiskTypes.CrossingHold:
                default:
                    break;
            }
        }

    }

    RiskTypes CalcRiskType(Ship otherShip)
    {
        float angDiff = Mathf.DeltaAngle(gameObject.transform.eulerAngles.y, otherShip.transform.eulerAngles.y);
        float speedDiff = ship.physics.GetSpeed() - otherShip.physics.GetSpeed();

        if (Mathf.Abs(angDiff) > 160)
        {
            return RiskTypes.HeadOn;
        }
        else if (Mathf.Abs(angDiff) < 5 && speedDiff > 0)
        {
            return RiskTypes.Overtaking;
        }
        else if (Mathf.Abs(angDiff) < 5 && speedDiff < 0)
        {
            return RiskTypes.Overtaken;
        }
        else if (angDiff < 0)
        {
            return RiskTypes.CrossingTurn;
        }
        else
        {
            return RiskTypes.CrossingHold;
        }
    }

    void CalcDesiredSpeedHeading(float dt)
    {
        if (desiredPositionList.Count > 0)
        {
            float dist = Vector3.Magnitude(gameObject.transform.position - desiredPositionList[0]);
            Vector3 totalForce = positionPFConstant * Mathf.Pow(dist, positionPFExponent) * Vector3.Normalize(desiredPositionList[0] - gameObject.transform.position);

            for(int a = 0; a < avoidancePFList.Count; a++)
            {
                var avoidancePFLocation = avoidancePFList[a];
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

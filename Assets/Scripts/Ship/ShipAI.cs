using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipAI : MonoBehaviour
{
    private Ship ship;

    public float minMagnitude;

    public List<Vector3> desiredPositionList;

    // Potential field at final location
    [SerializeField] private float positionPFConstant;
    [SerializeField] private float positionPFExponent;

    // Potential field to guide ship correctly
    [SerializeField] private List<Vector3> avoidancePFList = new List<Vector3>();
    [SerializeField] private List<float> avoidancePFMultiplierList = new List<float>();
    [SerializeField] private float avoidancePFConstant;
    [SerializeField] private float avoidancePFExponent;

    // New AI Constants
    [SerializeField] private float minTCPA; // The minimum TCPA for action to be taken
    [SerializeField] private float minDCPA;
    [SerializeField] private float maxDCPA;
    [SerializeField] private float minBearingDelta; // The minimum bearing change to see if the ships will collide
    [SerializeField] private float maxBearingDelta; // Used to determine if the ships will no longer collide

    public Vector3 prevPos { get; private set; }

    private void Awake()
    {
        ship = gameObject.GetComponent<Ship>();
        prevPos = gameObject.transform.position;
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

    Dictionary<int, RiskTypes> riskTypeDict = new Dictionary<int, RiskTypes>();
    enum RiskTypes {HeadOn, Overtaking, Overtaken, CrossingTurn, CrossingHold, None }
    void CheckForCollisions(float dt)
    {
        var newRiskTypeDict = new Dictionary<int, RiskTypes>();

        foreach(Ship otherShip in ShipMgr.instance.shipDict.Values)
        {
            float DCPA = ShipMgr.instance.cpaDict[ship.shipID][otherShip.shipID].cpaDist;
            float TCPA = ShipMgr.instance.cpaDict[ship.shipID][otherShip.shipID].CalcTCPA();

            // If there has been an update to the CPA list
            if (riskTypeDict.ContainsKey(otherShip.shipID) && TCPA > 0 && DCPA < maxDCPA)
            {
                newRiskTypeDict[otherShip.shipID] = riskTypeDict[otherShip.shipID];
            }
            if ( DCPA == DCPA  // Removes NaN distances
                && (!ShipMgr.instance.prevCpaDict.ContainsKey(ship.shipID)
                || !ShipMgr.instance.prevCpaDict[ship.shipID].ContainsKey(otherShip.shipID)
                || ShipMgr.instance.prevCpaDict[ship.shipID][otherShip.shipID].cpaDist != DCPA))
            {
                if (DCPA < minDCPA && TCPA < minTCPA)
                {
                    newRiskTypeDict[otherShip.shipID] = CalcRiskType(otherShip);
                }
            }
        }

        riskTypeDict = newRiskTypeDict;
        avoidancePFList = new List<Vector3>();
        avoidancePFMultiplierList = new List<float>();

        // Add potential fields
        foreach (int otherId in riskTypeDict.Keys)
        {
            Ship s = ShipMgr.instance.shipDict[otherId];
            Vector3 normalizedVel = Vector3.Normalize(s.physics.GetVelocity());

            switch (riskTypeDict[otherId])
            {
                case RiskTypes.HeadOn:
                    print("Head On");
                    var h = s.transform.position + 500 * new Vector3(-normalizedVel.z, 0, normalizedVel.x).normalized;
                    avoidancePFList.Add(h);
                    avoidancePFMultiplierList.Add(Mathf.Min(5, minTCPA / ShipMgr.instance.cpaDict[ship.shipID][otherId].CalcTCPA()));
                    break;

                case RiskTypes.Overtaking:
                    print("Overtaking");
                    var o = s.transform.position + 400 * new Vector3(normalizedVel.z, 0, -normalizedVel.x).normalized;
                    avoidancePFList.Add(o);
                    avoidancePFMultiplierList.Add(Mathf.Min(5, minTCPA / ShipMgr.instance.cpaDict[ship.shipID][otherId].CalcTCPA()));
                    break;

                case RiskTypes.CrossingTurn:
                    print("Crossing");
                    var c = s.transform.position + 500 * new Vector3(-normalizedVel.x, 0, -normalizedVel.z).normalized;
                    avoidancePFList.Add(c);
                    avoidancePFMultiplierList.Add(Mathf.Min(5, minTCPA / ShipMgr.instance.cpaDict[ship.shipID][otherId].CalcTCPA()));
                    break;

                //case RiskTypes.Overtaken:
                //case RiskTypes.CrossingHold:
                //case RiskTypes.None:
                default:
                    break;
            }
        }
    }

    RiskTypes CalcRiskType(Ship otherShip)
    {
        float angDiff = Mathf.DeltaAngle(gameObject.transform.eulerAngles.y, otherShip.transform.eulerAngles.y);
        float speedDiff = ship.physics.GetSpeed() - otherShip.physics.GetSpeed();
        Vector3 disLocation = otherShip.transform.position - gameObject.transform.position;
        float distDiff = Vector3.Magnitude(disLocation);
        float relAngle = Mathf.Abs(Mathf.Rad2Deg * Mathf.Acos(Vector3.Dot(disLocation, -otherShip.physics.GetVelocity()) / (disLocation.magnitude * -otherShip.physics.GetVelocity().magnitude)));

        if (Mathf.Abs(angDiff) > 160)
        {
            return RiskTypes.HeadOn;
        }
        else if (Mathf.Abs(angDiff) < 10 && speedDiff > 0)
        {
            return RiskTypes.Overtaking;
        }
        else if (Mathf.Abs(angDiff) < 10 && speedDiff < 0) 
        {
            return RiskTypes.Overtaken;
        }
        else if (angDiff < -10)
        {
            return RiskTypes.CrossingTurn;
        }
        else if (angDiff > 10)
        {
            return RiskTypes.CrossingHold;
        }
        return RiskTypes.None;
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
                totalForce += avoidancePFMultiplierList[a] * avoidancePFConstant * Mathf.Pow(avDist, avoidancePFExponent) * Vector3.Normalize(avoidancePFLocation - gameObject.transform.position);
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
            if (!CameraMgr.instance.controlled)
            {
                ship.physics.SetDesiredSpeed(0);
            }
        }
    }

}

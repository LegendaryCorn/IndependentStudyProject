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
        CalcDesiredSpeedHeading(Time.deltaTime);
    }

    void CheckDesiredPosition(float dt)
    {
        if (desiredPositionList.Count > 0 && Vector3.SqrMagnitude(transform.position - desiredPositionList[0]) < minMagnitude * minMagnitude)
        {
            desiredPositionList.RemoveAt(0);
        }
    }

    void CalcDesiredSpeedHeading(float dt)
    {
        if (desiredPositionList.Count > 0)
        {
            float dist = Vector3.Magnitude(gameObject.transform.position - desiredPositionList[0]);
            Vector3 totalForce = positionPFConstant * Mathf.Pow(dist, positionPFExponent) * Vector3.Normalize(desiredPositionList[0] - gameObject.transform.position);
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipAI : MonoBehaviour
{
    private Ship ship;

    public float minMagnitude;
    public List<Vector3> desiredPositionList;

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
            ship.physics.SetDesiredSpeed(Mathf.Infinity);
            Vector3 posDiff = desiredPositionList[0] - gameObject.transform.position;
            ship.physics.SetDesiredHeading(Mathf.Atan2(posDiff.x, posDiff.z));
        }
        else
        {
            ship.physics.SetDesiredSpeed(0);
        }
    }

}

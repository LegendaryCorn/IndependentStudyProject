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
        desiredPositionList.Add(pos);

        if(desiredPositionList.Count == 1)
        {
            ship.physics.desiredPosition = pos;
            ship.physics.hasDesiredPosition = true;
        }
    }

    public void ClearDesiredPositions()
    {
        desiredPositionList.Clear();
        ship.physics.hasDesiredPosition = false;
    }

    private void Update()
    {
        CheckDesiredPosition(Time.deltaTime);
    }

    void CheckDesiredPosition(float dt)
    {
        if (desiredPositionList.Count > 0 && Vector3.SqrMagnitude(transform.position - desiredPositionList[0]) < minMagnitude * minMagnitude)
        {
            desiredPositionList.RemoveAt(0);

            if (desiredPositionList.Count == 0) ship.physics.hasDesiredPosition = false;
            else ship.physics.desiredPosition = desiredPositionList[0];
        }
    }

}

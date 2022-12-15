using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScenarioShip : MonoBehaviour
{
    // Start positions
    public Vector3 startPoint;
    public Vector3 endPoint;
    public float startSpeed;
    public float startHeading;

    // Stats
    public float minSpeed;
    public float maxSpeed;
    public float acceleration;
    public float angularVelocity;
}

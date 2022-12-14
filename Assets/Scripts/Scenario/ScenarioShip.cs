using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScenarioShip : MonoBehaviour
{
    // Start positions
    public GameObject startPoint;
    public List<GameObject> waypoints;
    public float startSpeed;
    public float startHeading;

    // Stats
    public float minSpeed;
    public float maxSpeed;
    public float acceleration;
    public float angularVelocity;
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int teamID;

    public PlayerControl control;
    public PlayerSelection selection;

    private void Awake()
    {
        teamID = 0;
    }
}

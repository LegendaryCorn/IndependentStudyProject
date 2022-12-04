using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameMgr : MonoBehaviour
{

    public static GameMgr instance;

    public GameObject pref;

    private void Awake()
    {
        if(instance != null)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }

    void Start()
    {
        AIMgr.instance.GenerateField();
        AIMgr.instance.ShowField();
    }


    void Update()
    {
        
    }
}

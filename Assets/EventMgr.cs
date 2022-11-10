using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventMgr : MonoBehaviour
{
    public static EventMgr instance;

    public UnityEvent onPlayerJoin;
    public UnityEvent onPlayerLeave;

    public UnityEvent onDesiredSpeedChanged;
    public UnityEvent onDesiredHeadingChanged;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }

        onPlayerJoin = new UnityEvent();
        onPlayerLeave = new UnityEvent();

        onDesiredSpeedChanged = new UnityEvent();
        onDesiredHeadingChanged = new UnityEvent();
    }

    void Start()
    {

    }

    void Update()
    {

    }
}

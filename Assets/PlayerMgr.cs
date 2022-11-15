using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMgr : MonoBehaviour
{

    public static PlayerMgr instance;

    public ulong userID;
    public int teamID;
    public Dictionary<ulong, Player> playerDict;

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

        playerDict = new Dictionary<ulong, Player>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Player GetClientPlayer()
    {
        return playerDict[userID];
    }

    public bool CheckIfOnline()
    {
        return playerDict.ContainsKey(userID);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSelection : MonoBehaviour
{
    private Player player;

    public List<Ship> selectedShipList;

    private void Awake()
    {
        player = gameObject.GetComponent<Player>();
        selectedShipList = new List<Ship>();
    }

    public void AddShipToList(Ship s)
    {
        selectedShipList.Add(s);

        if(s.shipTeam == player.teamID)
        {
            s.friendlyMarker.SetActive(true);
        }
        else
        {
            s.enemyMarker.SetActive(true);
        }
    }

    public void RemoveShipFromList(Ship s)
    {
        try
        {
            selectedShipList.Remove(s);
            s.friendlyMarker.SetActive(false);
            s.enemyMarker.SetActive(false);
        }
        catch
        {

        }
    }

    public void ClearList()
    {
        foreach (Ship s in selectedShipList)
        {
            s.friendlyMarker.SetActive(false);
            s.enemyMarker.SetActive(false);
        }

        selectedShipList.Clear();
    }
}

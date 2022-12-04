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
        s.render.SelectShipRender(s.shipTeam == player.teamID);
    }

    public void RemoveShipFromList(Ship s)
    {
        try
        {
            selectedShipList.Remove(s);
            s.render.DeselectShipRender();
        }
        catch
        {

        }
    }

    public void ClearList()
    {
        foreach (Ship s in selectedShipList)
        {
            s.render.DeselectShipRender();
        }

        selectedShipList.Clear();
    }
}

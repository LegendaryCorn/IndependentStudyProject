using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipRender : MonoBehaviour
{
    private Ship ship;

    private readonly Color[] teamColors = { Color.blue, Color.red, Color.gray };

    [SerializeField] private MeshRenderer shipMarker;
    [SerializeField] private GameObject friendlyMarker;
    [SerializeField] private GameObject enemyMarker;

    private LineRenderer pathLines;

    private void Awake()
    {
        ship = gameObject.GetComponent<Ship>();

        var g = new GameObject();
        g.transform.parent = gameObject.transform;
        pathLines = g.AddComponent<LineRenderer>();
        pathLines.enabled = false;
        pathLines.positionCount = 0;
        pathLines.material = new Material(Shader.Find("Sprites/Default"));
        pathLines.material.color = Color.green;
        pathLines.startWidth = 2;
        pathLines.endWidth = 2;

        shipMarker.material.color = teamColors[ship.shipTeam];
    }

    public void SelectShipRender(bool friendly)
    {
        var m = friendly ? friendlyMarker : enemyMarker;
        m.SetActive(true);
        pathLines.enabled = true;
    }

    public void DeselectShipRender()
    {
        friendlyMarker.SetActive(false);
        enemyMarker.SetActive(false);
        pathLines.enabled = false;
    }

    private void RefreshPath()
    {
        if (pathLines.enabled)
        {
            Vector3[] arrayofPos = new Vector3[1 + ship.ai.desiredPositionList.Count];
            Vector3[] gp = { gameObject.transform.position };
            gp.CopyTo(arrayofPos, 0);
            ship.ai.desiredPositionList.ToArray().CopyTo(arrayofPos, 1);

            for(int i = 0; i < arrayofPos.Length; i++)
            {
                arrayofPos[i] += new Vector3(0, 5, 0);
            }

            pathLines.positionCount = arrayofPos.Length;
            pathLines.SetPositions(arrayofPos);
        }
    }

    private void Update()
    {
        RefreshPath();
    }
}

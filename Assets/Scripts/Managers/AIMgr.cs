using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIMgr : MonoBehaviour
{
    public static AIMgr instance;

    [SerializeField] private float width;
    [SerializeField] private float height;
    [SerializeField] private int numNodesWidth;
    [SerializeField] private int numNodesHeight;

    private Node[,] nodes;

    private LayerMask terrainMask;

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

        nodes = null;
        terrainMask = LayerMask.GetMask("Terrain");
    }


    void Update()
    {
        
    }

    public void GenerateField()
    {
        nodes = new Node[numNodesWidth, numNodesHeight];

        var distWidth = width / numNodesWidth;
        var distHeight = height / numNodesHeight;

        // Check if the nodes exist
        for (int i = 0; i < numNodesWidth; i++)
        {
            for (int j = 0; j < numNodesHeight; j++)
            {
                var pos = new Vector3((-width / 2) + (i * distWidth), 0, (-height / 2) + (j * distHeight));
                var posRay = new Ray(pos + 100 * Vector3.up, Vector3.down);
                nodes[i, j] = new Node(pos, !Physics.Raycast(posRay, Mathf.Infinity, terrainMask));


                //nodes[i, j] = new Node(pos, false, false, false, false);                
            }
        }

        // Check if the paths exist
        for (int i = 0; i < numNodesWidth; i++)
        {
            for (int j = 0; j < numNodesHeight; j++)
            {
                Node node = nodes[i, j];
                if (node.isValid)
                {
                    var pos = node.position;
                    var n = (j < numNodesHeight - 1 && nodes[i,j+1].isValid) ? !Physics.Raycast(pos, Vector3.forward, distHeight, terrainMask) : false;
                    var s = (j > 0 && nodes[i, j - 1].isValid) ? !Physics.Raycast(pos, Vector3.back, distHeight, terrainMask) : false;
                    var e = (i < numNodesWidth - 1 && nodes[i+1, j].isValid) ? !Physics.Raycast(pos, Vector3.right, distWidth, terrainMask) : false;
                    var w = (i > 0 && nodes[i-1, j].isValid) ? !Physics.Raycast(pos, Vector3.left, distWidth, terrainMask) : false;

                    nodes[i, j].SetPaths(n, s, e, w);
                }
            }
        }
    }

    public void ShowField()
    {
        var distWidth = width / numNodesWidth;
        var distHeight = height / numNodesHeight;

        for (int i = 0; i < numNodesWidth; i++)
        {
            for (int j = 0; j < numNodesHeight; j++)
            {
                Color c = nodes[i, j].isValid ? Color.green : Color.red;
                Debug.DrawRay(nodes[i, j].position, 5 * Vector3.up, c, 1000000.0f, false);
                if(nodes[i,j].pathNorth) Debug.DrawRay(nodes[i, j].position, distHeight * Vector3.forward, Color.yellow, 1000000.0f, false);
                if (nodes[i, j].pathSouth) Debug.DrawRay(nodes[i, j].position, distHeight * Vector3.back, Color.yellow, 1000000.0f, false);
                if (nodes[i, j].pathEast) Debug.DrawRay(nodes[i, j].position, distHeight * Vector3.right, Color.yellow, 1000000.0f, false);
                if (nodes[i, j].pathWest) Debug.DrawRay(nodes[i, j].position, distHeight * Vector3.left, Color.yellow, 1000000.0f, false);
            }
        }
    }

    public List<Vector3> GeneratePath(Vector3 startPos, Vector3 endPos)
    {
        
        List<Vector3> pathList = new List<Vector3>();

        // Check if there is a direct path already (no fancy math needed)
        if (!Physics.Linecast(startPos, endPos, terrainMask))
        {
            pathList.Add(startPos);
            pathList.Add(endPos);
            return pathList;
        }

        // Get closest points
        int[] startClosestPoint = GetClosestPoint(startPos);
        int[] endClosestPoint = GetClosestPoint(endPos);

        // Do A*
        float[,] pointValue = new float[numNodesWidth, numNodesHeight];
        for(int i = 0; i < numNodesWidth; i++)
        {
            for(int j = 0; j < numNodesHeight; j++)
            {
                pointValue[i, j] = Mathf.Infinity;
            }
        }
        pointValue[startClosestPoint[0], startClosestPoint[1]] = TaxiCabDist(startClosestPoint, endClosestPoint);

        // Flatten

        

        return pathList;
    }

    private int[] GetClosestPoint(Vector3 v)
    {
        float minSquaredDist = float.MaxValue;
        int[] minPoint = new int[2];

        for (int i = 0; i < numNodesWidth; i++)
        {
            for(int j = 0; j < numNodesHeight; j++)
            {
                Node n = nodes[i, j];
                if(n.isValid && Vector3.SqrMagnitude(v - n.position) < minSquaredDist && Physics.Linecast(v, n.position, terrainMask))
                {
                    minSquaredDist = Vector3.SqrMagnitude(v - n.position);
                    minPoint[0] = i;
                    minPoint[1] = j;
                }
            }
        }
        return minPoint;
    }

    private int TaxiCabDist(int[] s, int[] e)
    {
        return Mathf.Abs(s[0] - e[0]) + Mathf.Abs(s[1] - e[1]);
    }

    public struct Node
    {
        public Node(Vector3 p, bool v)
        {
            position = p;
            isValid = v;

            pathNorth = false;
            pathSouth = false;
            pathEast = false;
            pathWest = false;
        }

        public void SetPaths(bool n, bool s, bool e, bool w)
        {
            pathNorth = n;
            pathSouth = s;
            pathEast = e;
            pathWest = w;
        }

        public Vector3 position { get; private set; }
        public bool pathNorth { get; private set; }
        public bool pathSouth { get; private set; }
        public bool pathEast { get; private set; }
        public bool pathWest { get; private set; }
        public bool isValid { get; private set; }
    }
}

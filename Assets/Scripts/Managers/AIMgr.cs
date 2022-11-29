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

    public List<PotentialField> potentialFields;

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
        potentialFields = new List<PotentialField>();
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
                nodes[i, j] = new Node(pos, !Physics.Raycast(posRay, Mathf.Infinity, terrainMask), i, j);


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
        var startClosestNode = GetClosestNode(startPos);
        var endClosestNode = GetClosestNode(endPos);

        // Do A*
        float[,] pointValue = new float[numNodesWidth, numNodesHeight];
        float[,] pathLength = new float[numNodesWidth, numNodesHeight];
        Node[,] prevNode = new Node[numNodesWidth, numNodesHeight];

        for(int i = 0; i < numNodesWidth; i++)
        {
            for(int j = 0; j < numNodesHeight; j++)
            {
                pointValue[i, j] = Mathf.Infinity;
            }
        }
        pointValue[startClosestNode.coordX, startClosestNode.coordY] = TaxiCabDist(startClosestNode, endClosestNode);
        pathLength[startClosestNode.coordX, startClosestNode.coordY] = 0;
        prevNode[startClosestNode.coordX, startClosestNode.coordY] = startClosestNode;
        bool pathFound = false;

        List<Node> consideredNodes = new List<Node>();
        consideredNodes.Add(startClosestNode);

        while(true)
        {
            // Find lowest considered point
            Node lowestNode = startClosestNode;
            float lowestValue = Mathf.Infinity;
            foreach(Node n in consideredNodes)
            {
                if(pointValue[n.coordX, n.coordY] < lowestValue)
                {
                    lowestNode = n;
                    lowestValue = pointValue[n.coordX, n.coordY];
                }
            }

            // If no lowest node was found
            if(lowestValue == Mathf.Infinity)
            {
                break;
            }

            // Check if lowest considered point is the end
            if (lowestNode.Equals(endClosestNode))
            {
                pathFound = true;
                break;
            }

            // Add new points and calculate point heuristic
            consideredNodes.Remove(lowestNode);

            // North
            if (lowestNode.pathNorth)
            {
                var nodeNorth = nodes[lowestNode.coordX, lowestNode.coordY + 1];
                if (pointValue[nodeNorth.coordX, nodeNorth.coordY] == Mathf.Infinity)
                {
                    consideredNodes.Add(nodeNorth);
                    prevNode[nodeNorth.coordX, nodeNorth.coordY] = lowestNode;
                    pathLength[nodeNorth.coordX, nodeNorth.coordY] = pathLength[lowestNode.coordX, lowestNode.coordY] + 1;
                    pointValue[nodeNorth.coordX, nodeNorth.coordY] = pathLength[nodeNorth.coordX, nodeNorth.coordY] + TaxiCabDist(nodeNorth, endClosestNode);
                }
            }

            // South
            if (lowestNode.pathSouth)
            {
                var nodeSouth = nodes[lowestNode.coordX, lowestNode.coordY - 1];
                if (pointValue[nodeSouth.coordX, nodeSouth.coordY] == Mathf.Infinity)
                {
                    consideredNodes.Add(nodeSouth);
                    prevNode[nodeSouth.coordX, nodeSouth.coordY] = lowestNode;
                    pathLength[nodeSouth.coordX, nodeSouth.coordY] = pathLength[lowestNode.coordX, lowestNode.coordY] + 1;
                    pointValue[nodeSouth.coordX, nodeSouth.coordY] = pathLength[nodeSouth.coordX, nodeSouth.coordY] + TaxiCabDist(nodeSouth, endClosestNode);
                }
            }

            // East
            if (lowestNode.pathEast)
            {
                var nodeEast = nodes[lowestNode.coordX + 1, lowestNode.coordY];
                if (pointValue[nodeEast.coordX, nodeEast.coordY] == Mathf.Infinity)
                {
                    consideredNodes.Add(nodeEast);
                    prevNode[nodeEast.coordX, nodeEast.coordY] = lowestNode;
                    pathLength[nodeEast.coordX, nodeEast.coordY] = pathLength[lowestNode.coordX, lowestNode.coordY] + 1;
                    pointValue[nodeEast.coordX, nodeEast.coordY] = pathLength[nodeEast.coordX, nodeEast.coordY] + TaxiCabDist(nodeEast, endClosestNode);
                }
            }

            // West
            if (lowestNode.pathWest)
            {
                var nodeWest = nodes[lowestNode.coordX - 1, lowestNode.coordY];
                if (pointValue[nodeWest.coordX, nodeWest.coordY] == Mathf.Infinity)
                {
                    consideredNodes.Add(nodeWest);
                    prevNode[nodeWest.coordX, nodeWest.coordY] = lowestNode;
                    pathLength[nodeWest.coordX, nodeWest.coordY] = pathLength[lowestNode.coordX, lowestNode.coordY] + 1;
                    pointValue[nodeWest.coordX, nodeWest.coordY] = pathLength[nodeWest.coordX, nodeWest.coordY] + TaxiCabDist(nodeWest, endClosestNode);
                }
            }
        }

        // Create path list
        if (pathFound)
        {
            pathList.Insert(0, endPos);
            var nextNode = endClosestNode;
            for(int i = 0; i < 100; i++)
            {
                pathList.Insert(0, nextNode.position);

                nextNode = prevNode[nextNode.coordX, nextNode.coordY];

                if (nextNode.position == startClosestNode.position)
                {
                    pathList.Insert(0, nextNode.position);
                    break;
                }
            }
            pathList.Insert(0, startPos);
        }
        else
        {
            return null;
        }

        // Flatten path list
        List<Vector3> newPathList = new List<Vector3>();
        int prevInd = 0;
        newPathList.Add(startPos);
        for(int i = 0; i < pathList.Count; i++)
        {
            if (Physics.Linecast(pathList[prevInd], pathList[i], terrainMask))
            {
                i--;
                prevInd = i;
                newPathList.Add(pathList[i]);
            }
        }
        newPathList.Add(endPos);

        return newPathList;
    }

    private Node GetClosestNode(Vector3 v)
    {
        float minSquaredDist = float.MaxValue;
        Node minNode = nodes[0,0];

        for (int i = 0; i < numNodesWidth; i++)
        {
            for(int j = 0; j < numNodesHeight; j++)
            {
                Node n = nodes[i, j];
                if(n.isValid && Vector3.SqrMagnitude(v - n.position) < minSquaredDist && !Physics.Linecast(v, n.position, terrainMask))
                {
                    minSquaredDist = Vector3.SqrMagnitude(v - n.position);
                    minNode = n;
                }
            }
        }
        return minNode;
    }

    private int TaxiCabDist(Node s, Node e)
    {
        return Mathf.Abs(s.coordX - e.coordX) + Mathf.Abs(s.coordY - e.coordY);
    }

    public struct Node
    {
        public Node(Vector3 p, bool v, int x, int y)
        {
            coordX = x;
            coordY = y;

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
        public int coordX { get; private set; }
        public int coordY { get; private set; }
        public Vector3 position { get; private set; }
        public bool pathNorth { get; private set; }
        public bool pathSouth { get; private set; }
        public bool pathEast { get; private set; }
        public bool pathWest { get; private set; }
        public bool isValid { get; private set; }
    }
}

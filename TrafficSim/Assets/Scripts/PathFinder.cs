using Unity.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Jobs;
using System.Collections;

public class PathFinder : MonoBehaviour
{
    public Node endNode;
    public Node startNode;

    List<Node> openList;
    List<Node> closedList;

    List<Node> shortestPath;

    public Transform nodeContainer;

    [HideInInspector]
    public float distance;
    
    bool finding = false;
    
    [HideInInspector]
    public List<Node> currentPath;

    public void Awake()
    {
        openList = new List<Node>();
        closedList = new List<Node>();
    }

    public void FindPath()
    {
        if(!finding)
            StartCoroutine(FindPathCoroutine());
    }

    public List<Node> FindPath(Node startNode,Node endNode)
    {
        StopAllCoroutines();
        if (true)
        {
            if (startNode == null || endNode == null) return null;
            List<Node> openListTemp = new List<Node>();
            List<Node> closedListTemp = new List<Node>();
            Initialize(openListTemp,closedListTemp,startNode);
            while (openListTemp.Count > 0)
            {
                Node currentNode = GetLowestFCostNode(openListTemp);
                if (currentNode == endNode)
                {
                    //reached :)
                    distance = (endNode.gCost) / 100000f;
                    //Debug.Log(distance + " km");
                    return CalculatePath(endNode);
                }
                openListTemp.Remove(currentNode);
                closedListTemp.Add(currentNode);

                for (int i =0; i<currentNode.branches.Count;i++)
                {
                    Node branch = currentNode.branches[i];
                    if (closedListTemp.Contains(branch) || branch == null) continue;

                    // if not traversible --> add branch to closed list, continue

                    //
                    int multiplier = (branch.nodeType == Node.NodeType.divert) ? 120 : 100;
                    int newGCost = currentNode.gCost + CalculateDistance(currentNode, branch) * multiplier;
                    if (newGCost < branch.gCost)
                    {
                        branch.cameFrom = currentNode;
                        branch.gCost = newGCost;
                        branch.hCost = (100 * CalculateDistance(branch, endNode));///30;
                        branch.fCost = CalculateFCost(branch);

                        if (!openListTemp.Contains(branch))
                        {
                            openListTemp.Add(branch);
                        }
                    }
                }
            }
            //could not find path
            return null;
        }
        else
        {
            return null;
        }
    }

    private List<Node> CalculatePath(Node endNode)
    {
        List<Node> path = new List<Node>();
        path.Add(endNode);
        Node n = endNode;
        while (n.cameFrom != null)
        {
            path.Add(n.cameFrom);
            n = n.cameFrom;
        }
        path.Reverse();
        return path;
    }
    
    private Node GetLowestFCostNode(List<Node> openList)
    {
        Node node = openList[0];
        for (int i = 0; i < openList.Count; i++)
        {
            if (openList[i].fCost < node.fCost)
            {
                node = openList[i];
            }
        }
        return node;
    }

    public void Initialize(List<Node> openList,List<Node> closedList,Node startNode)
    {
        openList.Clear();
        openList.Add(startNode);
        closedList.Clear();
        foreach (Node node in GetComponentsInChildren<Node>())
        {
            node.gCost = int.MaxValue;
            node.cameFrom = null;
        }

        startNode.gCost = 0;
        startNode.hCost = (100 * CalculateDistance(startNode, endNode));///30;
        startNode.fCost = CalculateFCost(startNode);
    }

    public int CalculateDistance(Node from,Node to)
    {
        float distSqr = ((from.transform.position.x - to.transform.position.x) * (from.transform.position.x - to.transform.position.x) +
        (from.transform.position.z - to.transform.position.z) * (from.transform.position.z - to.transform.position.z));
        return (int) math.sqrt(distSqr);
    }

    public int CalculateFCost(Node n)
    {
        return n.gCost + n.hCost;
    }

    public void DrawPathMesh(List<Node> path,MeshFilter meshFilter,float width)
    {
        if (path == null) { meshFilter.mesh.Clear(); return; }
        List<Vector3> meshVerts = new List<Vector3>();
        List<int> meshTriangles = new List<int>();
        List<Vector2> uvList = new List<Vector2>();
        float blockLength = 0;
        for (int i = 0; i < path.Count; i++)
        {
            // Draw quad betwwen current and next path point
            int ind = meshVerts.Count;
            if (true)// if smoothen
            {
                if (i > 2)
                {
                    path[i - 1].transform.rotation = Quaternion.Slerp(
                        Quaternion.LookRotation(path[i].transform.position - path[i - 1].transform.position),
                        Quaternion.LookRotation(path[i - 1].transform.position - path[i - 2].transform.position),
                        0.5f
                        );
                }
            }
            meshVerts.AddRange(EvaluateVertexPos(path[i],width));
            if (i < path.Count - 1)
            {
                meshTriangles.AddRange(QuadTriangles(ind, ind + 1, ind + 3, ind + 2));
            }
            Vector2[] uvs = new Vector2[2];
            uvs[0] = new Vector2(1, blockLength);
            uvs[1] = new Vector2(0, blockLength);
            uvList.AddRange(uvs);
            if (i < path.Count - 1) blockLength += (path[i].transform.position - path[i + 1].transform.position).magnitude;
        }
        if (meshFilter.mesh != null)
            meshFilter.mesh.Clear();
        Mesh mesh = meshFilter.sharedMesh;
        mesh.vertices = meshVerts.ToArray();
        mesh.uv = uvList.ToArray();
        mesh.triangles = meshTriangles.ToArray();
        meshFilter.mesh = mesh;
    }

    public Vector3[] EvaluateVertexPos(Node node,float width)
    {
        Vector3[] verts = new Vector3[2];
        verts[0] = node.transform.position + 0.5f * node.transform.right * width;
        verts[1] = node.transform.position - 0.5f * node.transform.right * width;
        return verts;
    }

    int[] QuadTriangles(int a, int b, int c, int d)
    {
        return new int[] { a, b, c, a, c, d };
    }


    public IEnumerator FindPathCoroutine()
    {
        finding = true;
        if (startNode == null || endNode == null) currentPath = null;
        else
        {
            Initialize(openList,closedList,startNode);
            int nodesProcessed = 0;
            bool found = false;
            while (openList.Count > 0)
            {
                Node currentNode = GetLowestFCostNode(openList);
                if (currentNode == endNode)
                {
                    //reached :)
                    distance = (endNode.gCost) / 100000f;
                    //Debug.Log(distance + " km");
                    currentPath = CalculatePath(endNode);
                    found = true;
                }
                openList.Remove(currentNode);
                closedList.Add(currentNode);

                for (int i = 0; i < currentNode.branches.Count; i++)
                {
                    Node branch = currentNode.branches[i];
                    if (closedList.Contains(branch) || branch == null) continue;

                    // if not traversible --> add branch to closed list, continue

                    //
                    int multiplier = (branch.nodeType == Node.NodeType.divert) ? 120 : 100;
                    int newGCost = currentNode.gCost + CalculateDistance(currentNode, branch) * multiplier;
                    if (newGCost < branch.gCost)
                    {
                        branch.cameFrom = currentNode;
                        branch.gCost = newGCost;
                        branch.hCost = (100 * CalculateDistance(branch, endNode));///30;
                        branch.fCost = CalculateFCost(branch);

                        if (!openList.Contains(branch))
                        {
                            openList.Add(branch);
                        }
                    }
                }
                nodesProcessed += 1;
                if (nodesProcessed > 20)
                {
                    nodesProcessed = 0;
                    yield return null;
                }
            }
            if (!found)
            {
                //could not find path
                currentPath = null;
            }
        }
        finding = false ;
    }
}

public struct FindPathJob : IJob
{
    [ReadOnly] public NativeArray<Vector3> positions;

    [ReadOnly] public NativeMultiHashMap<int, int> branches;

    [ReadOnly] public NativeArray<int> multiplier;

    public NativeArray<int> cameFrom;
    public NativeArray<int> gCost;
    public NativeArray<int> hCost;
    public NativeArray<int> fCost;

    public int startIndex;
    public int endIndex;
    public NativeList<int> openList;
    public NativeList<int> closedList;

    public NativeList<int> calculatedPath;

    public void Execute()
    {
        Initialize();
        while (openList.Length > 0)
        {
            int currentNodeIndex = GetLowestFCostNode(openList);
            if (currentNodeIndex == endIndex)
            {
                //reached :)
                //distance = (gCost[endIndex]) / 100000f;
                //Debug.Log(distance + " km");
                CalculatePath(endIndex);
                break;
            }
            for (int i = 0; i < openList.Length; i++)
            {
                if (openList[i] == currentNodeIndex)
                {
                    openList.RemoveAt(i);
                    break;
                }
            }
            //openList.Remove(currentNodeIndex);
            closedList.Add(currentNodeIndex);

            IEnumerator<int> enumerator = branches.GetValuesForKey(currentNodeIndex);
            while (branches.GetValuesForKey(currentNodeIndex).MoveNext())
            {
                int j = enumerator.Current;
                if (closedList.Contains(j)) continue;

                // if not traversible --> add branch to closed list, continue

                //
                //int multiplier = (j.nodeType == Node.NodeType.divert) ? 120 : 100;
                int newGCost = gCost[currentNodeIndex] + CalculateDistance(currentNodeIndex, j) * multiplier[j];
                if (newGCost < gCost[j])
                {
                    cameFrom[j] = currentNodeIndex;
                    gCost[j] = newGCost;
                    hCost[j] = (100 * CalculateDistance(j, endIndex));///30;
                    fCost[j] = CalculateFCost(j);

                    if (!openList.Contains(j))
                    {
                        openList.Add(j);
                    }
                }
            }

            //foreach (var j in branches.GetValuesForKey(currentNodeIndex))
            //{
            //    if (closedList.Contains(j)) continue;

            //    // if not traversible --> add branch to closed list, continue

            //    //
            //    //int multiplier = (j.nodeType == Node.NodeType.divert) ? 120 : 100;
            //    int newGCost = gCost[currentNodeIndex] + CalculateDistance(currentNodeIndex, j) * multiplier[j];
            //    if (newGCost < gCost[j])
            //    {
            //        cameFrom[j] = currentNodeIndex;
            //        gCost[j] = newGCost;
            //        hCost[j] = (100 * CalculateDistance(j, endIndex));///30;
            //        fCost[j] = CalculateFCost(j);

            //        if (!openList.Contains(j))
            //        {
            //            openList.Add(j);
            //        }
            //    }
            //}
        }
        cameFrom.Dispose();
        gCost.Dispose();
        hCost.Dispose();
        fCost.Dispose();
        //openList.Dispose();
        //closedList.Dispose();
        positions.Dispose();
        branches.Dispose();
        multiplier.Dispose();
    }
    public void Initialize()
    {
        calculatedPath.Clear();
        openList.Add(startIndex);
        //closedList = new NativeList<int>();
        for (int i = 0; i < positions.Length; i++)
        {
            gCost[i] = int.MaxValue;
            cameFrom[i] = -1;
        }

        gCost[startIndex] = 0;
        hCost[startIndex] = (100 * CalculateDistance(startIndex, endIndex));///30;
        fCost[startIndex] = CalculateFCost(startIndex);
    }

    private int GetLowestFCostNode(NativeList<int> openList)
    {
        int fCost0 = fCost[openList[0]];
        int j = 0;
        for (int i = 0; i < openList.Length; i++)
        {
            if (fCost[openList[i]] < fCost0)
            {
                fCost0 = fCost[openList[i]];
                j = i; 
            }
        }
        return j;
    }

    private void CalculatePath(int endNodeIndex)
    {
        calculatedPath.Clear();
        calculatedPath.Add(endNodeIndex);
        int n = endNodeIndex;
        while (cameFrom[n] > 0)
        {
            calculatedPath.Add(cameFrom[n]);
            n = cameFrom[n];
        }
        //calculatedPath.Reverse();
    }

    public int CalculateDistance(int from, int to)
    {
        float distSqr = ((positions[from].x - positions[to].x) * (positions[from].x - positions[to].x) +
        (positions[from].z - positions[to].z) * (positions[from].z - positions[to].z));
        return (int)math.sqrt(distSqr);
    }

    public int CalculateFCost(int n)
    {
        return gCost[n] + hCost[n];
    }
}

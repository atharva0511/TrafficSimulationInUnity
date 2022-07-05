
using System.Collections.Generic;
using UnityEngine;

public class TrafficSimulator : MonoBehaviour
{
    [Tooltip("Max no. of vehicles in 100m radius region")]
    public int derfaultTrafficDensity = 5;
    public NodeSettings nodeSettings;
    public enum GizmoType { Line,Arrow,None};
    public GizmoType gizmoType = GizmoType.Line;
    public Transform trafficZonesContainer;
    public Transform NodeContainer;
    [HideInInspector]
    public Node prevNode;
    public int[] speedTierLimits = { 20,30,60,75};
    List<Node> path;

    Color speed1Col = new Color(1, 0.2f, 0, 0.25f);
    Color speed2Col = new Color(1f, 0.5f, 0, 0.25f);
    Color speed3Col = new Color(1, 1, 0, 0.25f);
    Color speed4Col = new Color(0.5f, 1, 0, 0.25f);

    public void OnDrawGizmos()
    {
        //draw node connections gizmo
        if (gizmoType!=GizmoType.None)
        {
            if (NodeContainer.GetComponentsInChildren<Node>().Length != 0)
            {
                foreach (Node node in NodeContainer.GetComponentsInChildren<Node>())
                {
                    if (node.branches.Count == 0) continue;
                    foreach (Node branch in node.branches)
                    {
                        if (branch == null) continue;
                        Gizmos.color = SpeedTierColor(branch.speedTier, nodeSettings);//NodeTypeColor(branch.nodeType, nodeSettings);
                        if (gizmoType == GizmoType.Arrow)
                        {
                            Vector3 dir = branch.transform.position - node.transform.position;
                            Vector3 perp = Vector3.Cross(dir, Vector3.up).normalized;
                            Vector3 pivot = Vector3.Lerp(branch.transform.position, node.transform.position, 0.25f);
                            Gizmos.DrawLine(node.transform.position, branch.transform.position);
                            Gizmos.DrawLine(pivot, pivot - dir.normalized + perp * 0.25f);
                            Gizmos.DrawLine(pivot, pivot - dir.normalized - perp * 0.25f);
                        }
                        else if(gizmoType == GizmoType.Line)
                        {
                            Gizmos.DrawLine(node.transform.position, branch.transform.position);
                        }
                    }
                }
            }
        }
        
        //draw path gizmo
        if (path!=null)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < path.Count-1; i++)
            {
                Gizmos.DrawLine(path[i].transform.position, path[i + 1].transform.position);
            }
        }
    }

    public List<Node> FindPath()
    {
        PathFinder pf = GetComponent<PathFinder>();
        path = pf.FindPath(pf.startNode,pf.endNode);
        return path;
    }

    public Color SpeedTierColor(Node.SpeedTier tier, NodeSettings ns)
    {
        Color col;
        switch (tier)
        {
            case Node.SpeedTier.low:
                col = ns == null ? speed1Col : ns.lowSpeedTierColor;
                break;
            case Node.SpeedTier.medium:
                col = ns == null ? speed2Col : ns.mediumSpeedTierColor;
                break;
            case Node.SpeedTier.high:
                col = ns == null ? speed3Col : ns.highSpeedTierColor;
                break;
            case Node.SpeedTier.express:
                col = ns == null ? speed4Col : ns.expressSpeedTierColor;
                break;
            default:
                col = new Color(1f, 0.5f, 0);
                break;
        }
        return col;
    }
}

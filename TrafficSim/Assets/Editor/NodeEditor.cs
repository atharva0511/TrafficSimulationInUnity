using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Node))]
public class NodeEditor : Editor
{
    public static Node selectedNode;

    [DrawGizmo(GizmoType.NonSelected | GizmoType.Selected | GizmoType.Pickable)]
    public static void OnDrawSceneGizmo(Node node,GizmoType gizmoType)
    {
        float size = 0.4f;
        TrafficSimulator sim = node.GetComponentInParent<TrafficSimulator>();
        if ((gizmoType & GizmoType.Selected) != 0)
        {
            Gizmos.color = NodeTypeColor(node.nodeType,sim.nodeSettings);
            size = 0.6f;
        }
        else
        {
            Gizmos.color = NodeTypeColor(node.nodeType,sim.nodeSettings)*0.5f;
        }
        Gizmos.DrawSphere(node.transform.position, size);

        if (node == selectedNode)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(node.transform.position, 1f);
            Vector3 pos = GetMouseWorldPos();
            Gizmos.DrawLine(node.transform.position, pos!=Vector3.zero?GetMouseWorldPos():node.transform.position);
        }
    }

    void OnSceneGUI()
    {
        Event e = Event.current;
        TrafficSimulator sim = ((Node)target).GetComponentInParent<TrafficSimulator>();
        NodeSettings ns = sim.nodeSettings;
        switch (e.type)
        {
            case EventType.KeyDown:
                {
                    if (ns!=null? Event.current.keyCode == ns.addNode: Event.current.keyCode == KeyCode.LeftAlt)
                    {
                        if (Selection.activeGameObject != null)
                        {
                            Node point = Selection.activeGameObject.GetComponent<Node>();
                            if (point != null)
                            {
                                AddNodePoint(point,sim.NodeContainer);
                            }
                        }
                    }
                    if(ns != null ? Event.current.keyCode == ns.connectNode : Event.current.keyCode == KeyCode.C)
                    {
                        // mark point
                        if (Selection.activeGameObject != null)
                        {
                            Node point = Selection.activeGameObject.GetComponent<Node>();
                            if (point != null)
                            {
                                if (selectedNode == null)
                                {
                                    selectedNode = point;
                                    //Gizmos.color = Color.red;
                                    //Gizmos.DrawWireSphere(selectedNode.transform.position, 1f);
                                }
                                else if(selectedNode!=point)
                                {
                                    selectedNode.branches.Add(point);
                                    selectedNode = null;
                                }
                                else
                                {
                                    selectedNode = null;
                                }
                            }
                        }
                    }
                    if(ns != null ? Event.current.keyCode == ns.markSpeedTier1 : Event.current.keyCode == KeyCode.Alpha1)
                    {
                        if (Selection.activeGameObject != null)
                        {
                            Node point = Selection.activeGameObject.GetComponent<Node>();
                            if (point != null)
                            {
                                point.speedTier = Node.SpeedTier.low;
                            }
                        }
                    }
                    if (ns != null ? Event.current.keyCode == ns.markSpeedTier2 : Event.current.keyCode == KeyCode.Alpha2)
                    {
                        if (Selection.activeGameObject != null)
                        {
                            Node point = Selection.activeGameObject.GetComponent<Node>();
                            if (point != null)
                            {
                                point.speedTier = Node.SpeedTier.medium;
                            }
                        }
                    }
                    if (ns != null ? Event.current.keyCode == ns.markSpeedTier3 : Event.current.keyCode == KeyCode.Alpha3)
                    {
                        if (Selection.activeGameObject != null)
                        {
                            Node point = Selection.activeGameObject.GetComponent<Node>();
                            if (point != null)
                            {
                                point.speedTier = Node.SpeedTier.high;
                            }
                        }
                    }
                    if (ns != null ? Event.current.keyCode == ns.markSpeedTier4 : Event.current.keyCode == KeyCode.Alpha4)
                    {
                        if (Selection.activeGameObject != null)
                        {
                            Node point = Selection.activeGameObject.GetComponent<Node>();
                            if (point != null)
                            {
                                point.speedTier = Node.SpeedTier.express;
                            }
                        }
                    }
                    if (ns != null ? Event.current.keyCode == ns.createDiversion : Event.current.keyCode == KeyCode.V)
                    {
                        if (Selection.activeGameObject != null)
                        {
                            Node point = Selection.activeGameObject.GetComponent<Node>();
                            if (point != null)
                            {
                                if (selectedNode == null)
                                {
                                    selectedNode = point;
                                    //Gizmos.color = Color.red;
                                    //Gizmos.DrawWireSphere(selectedNode.transform.position, 1f);
                                }
                                else if (selectedNode != point)
                                {
                                    Node divNode = AddNodePoint(selectedNode, selectedNode.transform.parent);
                                    divNode.transform.position = Vector3.Lerp(selectedNode.transform.position, point.transform.position, 0.5f);
                                    divNode.branches.Add(point);
                                    divNode.nodeType = Node.NodeType.divert;
                                    selectedNode.branches.Add(divNode);
                                    selectedNode = null;
                                    Selection.activeGameObject = point.gameObject;
                                }
                                else
                                {
                                    selectedNode = null;
                                }
                            }
                        }
                    }
                    break;
                }
        }
    }

    public Node AddNodePoint(Node prevNode,Transform nodeContainer)
    {
        Vector3 pos = GetMouseWorldPos();
        if (pos == Vector3.zero) return null;
        GameObject nodeObject = new GameObject("Node " + nodeContainer.childCount, typeof(Node));
        
        //Set Transform
        nodeObject.transform.position = pos+Vector3.up*0.5f;
        nodeObject.transform.rotation = Quaternion.LookRotation(nodeObject.transform.position - prevNode.transform.position);
        // Copy previous node properties
        nodeObject.GetComponent<Node>().CopyProperties(prevNode);
        prevNode.branches.Add(nodeObject.GetComponent<Node>());

        nodeObject.transform.SetParent(nodeContainer);
        Selection.activeGameObject = nodeObject;

        return nodeObject.GetComponent<Node>();
    }

    public static Vector3 GetMouseWorldPos()
    {
        Ray mouseRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        RaycastHit raycastHit;
        if (Physics.Raycast(mouseRay, out raycastHit, 400)){
            return raycastHit.point;
        }
        else
            return Vector3.zero;
    }

    public static Color NodeTypeColor(Node.NodeType type, NodeSettings ns)
    {
        Color col;
        switch (type)
        {
            case Node.NodeType.divert:
                col = ns!=null?ns.diversionNodeColor: new Color(1, 0.5f, 0, 1);
                break;
            case Node.NodeType.uTurn:
                col = ns != null ? ns.parkingNodeColor: Color.red ;
                break;
            case Node.NodeType.parking:
                col = ns != null ? ns.parkingNodeColor: Color.red;
                break;
            default:
                col = ns != null ? ns.normalNodeColor: Color.yellow;
                break;
        }
        return col;
    }
}

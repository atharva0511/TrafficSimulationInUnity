using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TrafficSimulator))]
public class TrafficEditor : Editor
{

    void OnSceneGUI()
    {
        Event e = Event.current;
        TrafficSimulator sim = ((TrafficSimulator)target);
        NodeSettings ns = sim.nodeSettings;
        switch (e.type)
        {
            case EventType.KeyDown:
                {
                    if (ns != null ? Event.current.keyCode == ns.addNode : Event.current.keyCode == KeyCode.LeftAlt)
                    {
                        if (Selection.activeGameObject == sim.gameObject)
                        {
                            AddNodePoint(sim.NodeContainer);
                        }
                    }
                    break;
                }
        }
    }

    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Refresh All Nodes"))
        {
            int c = 0;
            int layer = LayerMask.NameToLayer("SpawnTriggers");
            TrafficSimulator sim = ((TrafficSimulator)target);
            foreach (Node node in sim.GetComponentsInChildren<Node>())
            {
                //remove empty branches
                for (int i = 0; i < node.branches.Count; i++)
                {
                    if (node.branches[i] == null) { node.branches.RemoveAt(i);c++; }
                }
                node.gameObject.layer = layer;

                //set speed limits according to tier
                switch (node.speedTier)
                {
                    case Node.SpeedTier.low:
                        node.speedLimit = sim.speedTierLimits[0];
                        break;
                    case Node.SpeedTier.medium:
                        node.speedLimit = sim.speedTierLimits[1];
                        break;
                    case Node.SpeedTier.high:
                        node.speedLimit = sim.speedTierLimits[2];
                        break;
                    case Node.SpeedTier.express:
                        node.speedLimit = sim.speedTierLimits[3];
                        break;
                    default:
                        node.speedLimit = sim.speedTierLimits[1];

                        break;
                }

                //set spawn triggers
                if (node.nodeType != Node.NodeType.divert && node.speedTier != Node.SpeedTier.low && node.nodeType != Node.NodeType.uTurn)
                {
                    Collider col = node.GetComponent<Collider>();
                    if (col != null)
                    {
                        DestroyImmediate(col);
                    }
                    col = node.gameObject.AddComponent(typeof(SphereCollider)) as SphereCollider;
                    col.isTrigger = true;

                }
                //set tag
                node.gameObject.tag = "Node";

                node.spawnDensity = sim.derfaultTrafficDensity;
            }
            Debug.Log(c.ToString() + " Empty branches removed");

            //Apply traffic zone settings
            for (int i = 0; i < sim.trafficZonesContainer.childCount; i++)
            {
                TrafficDensityZone tdz = sim.trafficZonesContainer.GetChild(i).GetComponent<TrafficDensityZone>();
                if (tdz != null)
                {
                    if (tdz.densityZoneShape == TrafficDensityZone.DensityZoneShape.Sphere)
                    {
                        Collider[] cols = Physics.OverlapSphere(tdz.transform.position, tdz.radius,1<<layer);
                        for (int j = 0; j < cols.Length; j++)
                        {
                            Node n = cols[j].GetComponent<Node>();
                            if (n != null)
                            {
                                n.spawnDensity = tdz.vehicleDensity;
                            }
                        }
                    }
                    else if(tdz.densityZoneShape == TrafficDensityZone.DensityZoneShape.Box)
                    {
                        Collider[] cols = Physics.OverlapBox(tdz.transform.position, 0.5f*tdz.bounds.size, tdz.transform.rotation, 1 << layer);
                        for (int j = 0; j < cols.Length; j++)
                        {
                            Node n = cols[j].GetComponent<Node>();
                            if (n != null)
                            {
                                n.spawnDensity = tdz.vehicleDensity;
                            }
                        }
                    }
                }
            }
        }

        if(GUILayout.Button("Find Path"))
        {
            ((TrafficSimulator)target).FindPath();
        }
        GUILayout.Label(" ");
        base.OnInspectorGUI();
    }

    public Node AddNodePoint(Transform nodeContainer)
    {
        Vector3 pos = NodeEditor.GetMouseWorldPos();
        if (pos == Vector3.zero) return null;
        GameObject nodeObject = new GameObject("Node " + nodeContainer.childCount, typeof(Node));

        //Set Transform
        nodeObject.transform.position = pos + Vector3.up * 0.5f;
        nodeObject.transform.rotation = Quaternion.identity;
        

        nodeObject.transform.SetParent(nodeContainer);
        Selection.activeGameObject = nodeObject;

        return nodeObject.GetComponent<Node>();
    }

}

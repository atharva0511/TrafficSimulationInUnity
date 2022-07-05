using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class RoadPoint : MonoBehaviour
{
    public byte lanes = 2;
    public bool autoRotate = true;
    public bool skipMeshDraw = false;
    public byte materialIndex = 0;
    public float stretchUV = 1;
    public float UVOffset = 0;
    public int smoothenAmount = 0;
    [HideInInspector]
    public Vector3 smoothenHandle;

    public bool setNodes = false;
    public Node.SpeedTier nodeSpeedTier = Node.SpeedTier.medium;

    public Dictionary<int,List<Node>> nodes;

    public void ClearNodes()
    {
        if (nodes!=null)
        {
            //for (int i = 0; i < nodes.Count; i++)
            //{
            //    if (nodes.ContainsKey(i))
            //    {
            //        for (int p = 0; p < nodes[i].Count; p++)
            //        {
            //            if (nodes[i] != null)
            //            {
            //                if (nodes[i][p] != null)
            //                    DestroyImmediate(nodes[i][p].gameObject);
            //            }
            //        }
            //    }
            //}
            foreach (KeyValuePair<int,List<Node>> entry in nodes)
            {
                for (int i = 0; i < entry.Value.Count; i++)
                {
                    if (entry.Value[i] != null)
                        DestroyImmediate(entry.Value[i].gameObject);
                } 
            }
        }
        nodes = new Dictionary<int, List<Node>>();
        for (int i = 0; i < lanes; i++)
        {
            nodes.Add(i, new List<Node>());
        }
    }

    public void OnDestroy()
    {
        ClearNodes();
        if(transform.GetSiblingIndex()>0)
            transform.parent.GetChild(transform.GetSiblingIndex() - 1).GetComponent<RoadPoint>().ClearNodes();
    }

    public float EvaluateNodeSpacing()
    {
        switch (nodeSpeedTier)
        {
            case Node.SpeedTier.high:
                return 8;
            case Node.SpeedTier.express:
                return 12;
            default:
                return 8;
        }
    }
}

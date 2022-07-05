using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public class Node : MonoBehaviour
{

    public List<Node> branches = new List<Node>();
    public bool signal = true;
    public int speedLimit = 25;
    public enum SpeedTier { low,medium,high,express}
    public SpeedTier speedTier = SpeedTier.medium;
    public int spawnDensity = 6; // vehicles per 100m dia

    public enum NodeType {normal,divert,parking,uTurn};

    public NodeType nodeType = NodeType.normal;

    [HideInInspector]
    public Node cameFrom = null;
    [HideInInspector]
    public int gCost;
    [HideInInspector]
    public int hCost;
    [HideInInspector]
    public int fCost;

    public void CopyProperties(Node node)
    {
        speedTier = node.speedTier;
        speedLimit = node.speedLimit;
    }
}

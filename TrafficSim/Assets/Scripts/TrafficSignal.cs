using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficSignal : MonoBehaviour
{
    public enum SignalType { fourWay,threeWay }
    public SignalType signalType = SignalType.threeWay;

    public Node[] nodes0;
    public Node[] nodes1;
    public Node[] nodes2;
    public Node[] nodes3;

    [Tooltip("Mesh renderer array (In order of node sets)")]
    public MeshRenderer[] meshRenderers;

    private void OnDrawGizmosSelected()
    {
        if (signalType == SignalType.threeWay)
        {
            if (nodes0.Length>0)
            {
                foreach (Node node in nodes0)
                {
                    if (node == null) continue;
                    Gizmos.color = node.signal ? Color.green : Color.red;
                    Gizmos.DrawLine(transform.position, node.transform.position);
                }
            }
            if (nodes1.Length > 0)
            {
                foreach (Node node in nodes1)
                {
                    if (node == null) continue;
                    Gizmos.color = node.signal ? Color.green : Color.red;
                    Gizmos.DrawLine(transform.position, node.transform.position);
                }
            }
            if (nodes2.Length > 0)
            {
                foreach (Node node in nodes2)
                {
                    if (node == null) continue;
                    Gizmos.color = node.signal ? Color.green : Color.red;
                    Gizmos.DrawLine(transform.position, node.transform.position);
                }
            }
        }
        else if(signalType == SignalType.fourWay)
        {
            if (nodes0.Length > 0)
            {
                foreach (Node node in nodes0)
                {
                    if (node == null) continue;
                    Gizmos.color = node.signal ? Color.green : Color.red;
                    Gizmos.DrawLine(transform.position, node.transform.position);
                }
            }
            if (nodes1.Length > 0)
            {
                foreach (Node node in nodes1)
                {
                    if (node == null) continue;
                    Gizmos.color = node.signal ? Color.green : Color.red;
                    Gizmos.DrawLine(transform.position, node.transform.position);
                }
            }
            if (nodes2.Length > 0)
            {
                foreach (Node node in nodes2)
                {
                    if (node == null) continue;
                    Gizmos.color = node.signal ? Color.green : Color.red;
                    Gizmos.DrawLine(transform.position, node.transform.position);
                }
            }
            if (nodes3.Length > 0)
            { 
                foreach (Node node in nodes3)
                {
                    if (node == null) continue;
                    Gizmos.color = node.signal ? Color.green : Color.red;
                    Gizmos.DrawLine(transform.position, node.transform.position);
                }
            }
        }
    }

    private void OnEnable()
    {
        switch (signalType)
        {
            case SignalType.fourWay:
                TrafficSignalManager.SignalChange4way += OnSignalChange4Way;
                break;
            case SignalType.threeWay:
                TrafficSignalManager.SignalChange3way += OnSignalChange3Way;
                break;
            default:
                break;
        }
    }

    private void OnDisable()
    {
        TrafficSignalManager.SignalChange4way -= OnSignalChange4Way;
        TrafficSignalManager.SignalChange3way -= OnSignalChange3Way;
    }

    private void OnSignalChange3Way(bool[] signals)
    {
        foreach (Node n in nodes0)
        {
            n.signal = signals[0];
        }
        foreach (Node n in nodes1)
        {
            n.signal = signals[1];
        }
        foreach (Node n in nodes2)
        {
            n.signal = signals[2];
        }

        ChangeLightColor(signals);
    }

    private void OnSignalChange4Way(bool[] signals)
    {
        foreach (Node n in nodes0)
        {
            n.signal = signals[0];
        }
        foreach (Node n in nodes1)
        {
            n.signal = signals[1];
        }
        foreach (Node n in nodes2)
        {
            n.signal = signals[2];
        }
        foreach (Node n in nodes3)
        {
            n.signal = signals[3];
        }

        ChangeLightColor(signals);
    }

    void ChangeLightColor(bool[] signals)
    {
        bool allRed = true;
        foreach (bool sig in signals)
        {
            allRed = !sig && allRed;
        }
        if (meshRenderers != null)
        {
            if (meshRenderers.Length >= signals.Length)
            {
                for (int i = 0; i < signals.Length; i++)
                {
                    if (meshRenderers[i] != null)
                        meshRenderers[i].material.SetFloat("ColorValue", signals[i] ? 1 : (allRed ? 2 : 0));
                }
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

[CustomEditor(typeof(RoadMaster))]
public class RoadMaker : Editor
{
    
    public override void OnInspectorGUI()
    {
        GUILayout.Label("Road Network Options");

        if (GUILayout.Button("Spawn New Roadway"))
        {
            Transform master = ((RoadMaster)target).transform;
            GameObject route = new GameObject("Roadway " + master.childCount);
            route.transform.SetParent(master);
            route.transform.position = master.position;
            GameObject roadPoint1 = new GameObject("RoadPoint0", typeof(RoadPoint));
            roadPoint1.transform.SetParent(route.transform);
            GameObject roadPoint2 = new GameObject("RoadPoint1", typeof(RoadPoint));
            roadPoint2.transform.SetParent(route.transform);
            roadPoint1.transform.position = master.position;
            roadPoint2.transform.position = master.position + 10 * Vector3.forward;
        }
        if (GUILayout.Button("Update Mesh"))
        {
            RoadMaster rm = ((RoadMaster)target);
            rm.DrawMesh();
            rm.DrawRadarMesh();
            rm.GetComponent<MeshCollider>().sharedMesh = rm.GetComponent<MeshFilter>().mesh;
            //((RoadMaster)target).GetComponent<MeshCollider>().sharedMesh = ((RoadMaster)target).GetComponent<MeshFilter>().mesh;
        }
        GUILayout.Label(" ");

        base.OnInspectorGUI();
    }

    public void OnSceneGUI()
    {
        ((RoadMaster)target).DrawMesh();
    }
    
}

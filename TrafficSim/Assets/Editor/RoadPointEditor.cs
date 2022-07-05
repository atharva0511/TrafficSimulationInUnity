using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RoadPoint))]
public class RoadPointEditor : Editor
{
    int prevSmoothenAmount = 0;

    [DrawGizmo(GizmoType.NonSelected | GizmoType.Selected | GizmoType.Pickable)]
    public static void OnDrawSceneGizmo(RoadPoint point, GizmoType gizmoType)
    {
        if ((gizmoType & GizmoType.Selected) != 0)
        {
            if (point.smoothenAmount > 0)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(point.smoothenHandle, 1f);
                Gizmos.DrawLine(point.transform.position, point.smoothenHandle);
            }
            Gizmos.color = Color.blue;
        }
        else
        {
            Gizmos.color = Color.blue * 0.5f;
        }
        Gizmos.DrawSphere(point.transform.position, 0.5f);
    }
    

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if(GUILayout.Button("Update Roadway"))
        {
            Transform roadway = ((RoadPoint)target).transform.parent;
            byte l = ((RoadPoint)target).lanes;
            foreach (RoadPoint point in roadway.GetComponentsInChildren<RoadPoint>())
            {
                point.lanes = l;
            }
            roadway.GetComponentInParent<RoadMaster>().DrawMesh();
        }

        if (GUILayout.Button("Straighten"))
        {
            RoadPoint point = ((RoadPoint)target);
            Transform roadway = point.transform.parent;
            if (point.transform.GetSiblingIndex() > 0)
            {
                RoadPoint prevPoint = roadway.GetChild(point.transform.GetSiblingIndex() - 1).GetComponent<RoadPoint>();
                if (prevPoint.smoothenAmount > 0)
                {
                    point.transform.rotation = Quaternion.LookRotation(point.transform.position - prevPoint.smoothenHandle);
                }
                else
                    point.transform.rotation = Quaternion.LookRotation(point.transform.position - prevPoint.transform.position);
            }
            
        }

        RoadPoint rp = ((RoadPoint)target);
        if (GUILayout.Button("Add/Update Traffic Nodes"))
        {
            rp = ((RoadPoint)target);
            rp.transform.GetComponentInParent<RoadMaster>().AddNodes(rp.transform.parent);
        }
        if (GUILayout.Button("Clear Traffic Nodes"))
        {
            rp = ((RoadPoint)target);
            rp.ClearNodes();
        }
        if (rp.smoothenHandle == Vector3.zero)
        {
            rp.smoothenHandle = rp.transform.position + rp.transform.forward * 3;
        }
    }

    void OnSceneGUI()
    {
        RoadPoint script = (RoadPoint)target;
        Event e = Event.current;
        switch (e.type)
        {
            case EventType.KeyDown:
                {
                    if (Event.current.keyCode == (KeyCode.LeftAlt))
                    {
                        if (Selection.activeGameObject != null)
                        {
                            RoadMaster m = Selection.activeGameObject.GetComponentInParent<RoadMaster>();
                            if (m != null)
                            {
                                RoadMaster master = script.GetComponentInParent<RoadMaster>();
                                RoadPoint point = Selection.activeGameObject.GetComponent<RoadPoint>();
                                if (point!=null)
                                    master.activeRoadway = point.transform.parent;
                                AddRoadPoint(master.activeRoadway);
                            }
                        }
                    }
                    break;
                }
        }
        if(script!=null)
            script.GetComponentInParent<RoadMaster>().DrawMesh();

        if (script.smoothenAmount > 0)
        {
            EditorGUI.BeginChangeCheck();
            Vector3 newTargetPosition = Handles.PositionHandle(script.smoothenHandle, Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(script, "Change Smoothen Handle Position");
                script.smoothenHandle = newTargetPosition;
            }
        }
    }


    public void AddRoadPoint(Transform activeRoadway)
    {
        GameObject roadPoint1 = new GameObject("RoadPoint " + activeRoadway.childCount, typeof(RoadPoint));

        Vector3 mousePosition = Event.current.mousePosition;
        mousePosition.y = SceneView.currentDrawingSceneView.camera.pixelHeight - mousePosition.y;
        mousePosition = SceneView.currentDrawingSceneView.camera.ScreenToWorldPoint(mousePosition);
        mousePosition.y = -mousePosition.y;
        //Set Transform
        roadPoint1.transform.position = GetMouseWorldPos();
        roadPoint1.transform.rotation = Quaternion.LookRotation(roadPoint1.transform.position - activeRoadway.transform.GetChild(activeRoadway.transform.childCount-1).position);
        // Copy previous point's properties
        RoadPoint prevPoint = activeRoadway.transform.GetChild(activeRoadway.transform.childCount - 1).GetComponent<RoadPoint>();
        roadPoint1.GetComponent<RoadPoint>().lanes = prevPoint.lanes;
        roadPoint1.GetComponent<RoadPoint>().autoRotate = prevPoint.autoRotate;
        roadPoint1.GetComponent<RoadPoint>().materialIndex = prevPoint.materialIndex;
        roadPoint1.GetComponent<RoadPoint>().setNodes = prevPoint.setNodes;
        roadPoint1.GetComponent<RoadPoint>().nodeSpeedTier = prevPoint.nodeSpeedTier;

        if (prevPoint.transform.GetSiblingIndex() > 0 && prevPoint.autoRotate)
        {
            Transform prevPoint2 = activeRoadway.GetChild(prevPoint.transform.GetSiblingIndex() - 1);
            prevPoint.transform.rotation = Quaternion.Slerp(prevPoint2.rotation, roadPoint1.transform.rotation, 0.5f);
        }

        roadPoint1.transform.SetParent(activeRoadway);
        Selection.activeGameObject = roadPoint1;
    }

    public Vector3 GetMouseWorldPos()
    {
        Ray mouseRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        RaycastHit raycastHit;
        if (Physics.Raycast(mouseRay, out raycastHit, 400))
        {
            return raycastHit.point;
        }
        else
            return Vector3.zero;
    }

}

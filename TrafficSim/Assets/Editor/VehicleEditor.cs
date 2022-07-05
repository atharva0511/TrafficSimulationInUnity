
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

[CustomEditor(typeof(VehicleController))]
public class VehicleEditor : Editor
{
    VehicleController controller;
    BoxBoundsHandle m_BoundsHandle = new BoxBoundsHandle();

    private void OnEnable()
    {
        controller = (VehicleController)target;
    }

    private void OnSceneGUI()
    {
            // draw the handle
        EditorGUI.BeginChangeCheck();
        m_BoundsHandle.DrawHandle();
        if (EditorGUI.EndChangeCheck())
        {
                // record the target object before setting new values so changes can be undone/redone
            Undo.RecordObject(controller, "Change Sensor Bounds");

                // copy the handle's updated data back to the target object
            //Bounds newBounds = new Bounds();
            //newBounds.center = m_BoundsHandle.center;
            //newBounds.size = m_BoundsHandle.size;
            //controller.bounds = newBounds;
        }
    }


}

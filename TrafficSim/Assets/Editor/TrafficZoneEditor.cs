
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

[CustomEditor(typeof(TrafficDensityZone))]
public class TrafficZoneEditor : Editor
{
    private BoxBoundsHandle m_BoundsHandle = new BoxBoundsHandle();

    private void OnSceneGUI()
    {
        TrafficDensityZone tdz = (TrafficDensityZone)target;
        if (tdz.densityZoneShape == TrafficDensityZone.DensityZoneShape.Sphere)
        {
            EditorGUI.BeginChangeCheck();
            Handles.color = Color.Lerp(Color.green, Color.red, tdz.vehicleDensity * 0.05f);
            float radius = Handles.RadiusHandle(Quaternion.identity, tdz.transform.position, tdz.radius);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(tdz, "Change Zone Radius");
                tdz.radius = radius;
            }
        }
        else if(tdz.densityZoneShape == TrafficDensityZone.DensityZoneShape.Box)
        {
            // copy the target object's data to the handle
            m_BoundsHandle.center = tdz.transform.position;
            m_BoundsHandle.size = tdz.bounds.size;

            EditorGUI.BeginChangeCheck();
            Handles.color = Color.Lerp(Color.green, Color.red, tdz.vehicleDensity * 0.05f);
            m_BoundsHandle.DrawHandle();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(tdz, "Change Zone Bounds");
                Bounds newBounds = new Bounds();
                newBounds.center = m_BoundsHandle.center;
                newBounds.size = m_BoundsHandle.size;
                tdz.bounds = newBounds;
            }
        }
    }
}

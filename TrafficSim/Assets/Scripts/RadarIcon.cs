using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public class RadarIcon : MonoBehaviour
{
    public string iconDescription = "";
    public bool boundInMiniMap = true;

    public bool dynamicIcon = false;
    public bool markable = true;

    //nearestNode: to be frequently updated by agent if dynamic icon is true
    Node nearestNode;

    //public void MarkIcon()
    //{
    //    marked = true;
    //    if (!MapManager.RadarIcons.Contains(this))
    //        MapManager.RadarIcons.Add(this);
    //}
    
    public float GetSnapRadius()
    {
        return GetComponent<SphereCollider>().radius;
    }

    private void Start()
    {
        if (!MapManager.RadarIcons.Contains(this))
            MapManager.RadarIcons.Add(this);
    }

    private void OnEnable()
    {
        if (MapManager.RadarIcons == null) return;
        if (!MapManager.RadarIcons.Contains(this))
            MapManager.RadarIcons.Add(this);
    }

    private void OnDisable()
    {
        if (MapManager.RadarIcons.Contains(this))
            MapManager.RadarIcons.Remove(this);
    }

    private void OnDestroy()
    {
        if (MapManager.RadarIcons.Contains(this))
            MapManager.RadarIcons.Remove(this);
    }

}

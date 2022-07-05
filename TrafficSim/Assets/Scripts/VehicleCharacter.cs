using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleCharacter : MonoBehaviour
{
    public string vehicleName;
    public bool godMode = false;
    public float health = 100;
    public bool isImportant = false;

    [Header("Components")]
    public Deform deformScript;
    public VehicleStatsManager vehStatsMan;
    VehicleStatsManager.VehicleStats vehicleStats;
    public Renderer rend;


    //public void OnSpawn()
    //{
    //        //deformScript.ResetDeformation();
    //    health = vehicleStats.health;
    //    rend.materials[0].color = RandomColor();
    //}


    void Awake()
    {
        foreach (VehicleStatsManager.VehicleStats veh in vehStatsMan.vehicleLibrary)
        {
            if (veh.name==(vehicleName))
            {
                vehicleStats = veh;
                break;
            }
        }
    }
    
    public void TakeDamage(float collisionImpact)
    {
        if (!godMode)
        {
            health -= 0.001f * vehicleStats.damageMultiplier * collisionImpact;
        }
    }

    public Color RandomColor()
    {
        return new Color(Random.value, Random.value, Random.value);
    }

}

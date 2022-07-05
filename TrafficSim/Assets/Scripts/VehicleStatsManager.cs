
using UnityEngine;
using System;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "VehicleStatsManager", menuName = "Custom Objects/VehicleStatsManager")]
public class VehicleStatsManager : ScriptableObject
{
    [Serializable]
    public class VehicleStats
    {
        public string name = "Destine";
        //performance
        public float maxSpeed = 40;
        public float acceleration = 3;

        //damage 
        [Range(0.1f,5f)]
        public float damageMultiplier = 1;
        [Range(1,5000)]
        public float health = 100;

    }

    public int vehicleDissapearDistance = 130;

    public List<VehicleStats> vehicleLibrary;
}

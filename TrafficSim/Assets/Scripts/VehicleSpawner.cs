using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class VehicleSpawner : MonoBehaviour
{
    List<GameObject> vehiclePool = new List<GameObject>();
    public bool displayDetails = false;
    public int vehicleQuantity = 20;
    public VehiclePopulation[] vehiclePrefabs;
    public LayerMask layerMask;
    public LayerMask vehicleLayer;
    public static int spawnedVeh = 0;
    public static int dissapearDistSqr = 40000;
    public static int physicsLODDistSqr = 6400;
    int vehPoolSize = 0;
    int nodeCount = 0;

    [Header("Vehicle Spawn Details")]
    public int minVehSpawnRadius = 75;
    public int maxVehSpawnRadius = 120;


    [Tooltip("Distance from camera above which vehicle will disappear")]
    public int dissapearDistance = 180;
    [Tooltip("Distance from camera above which wheel colliders will not be used")]
    public int physicsLODDistance = 80;

    [Range(3,10)]
    public int skipFrames = 5;
    int spawnFrame = 0;
    // Start is called before the first frame update
    void Start()
    {

        List<GameObject> vehicles = new List<GameObject>();
        for(int i =0; i<vehiclePrefabs.Length; i++)
        {
            for(int j = 0; j < vehiclePrefabs[i].amount; j++)
            {
                vehicles.Add(vehiclePrefabs[i].prefab);
                vehPoolSize += 1;
            }
        }
        ObjectPooler.instance.InitializeVehPool("Vehicles", vehicles.ToArray());

        dissapearDistSqr = dissapearDistance*dissapearDistance;
        physicsLODDistSqr = physicsLODDistance*physicsLODDistance;

        StartCoroutine(SpawnRoutine());
    }

    private void OnGUI()
    {
        if (displayDetails)
        {
            GUI.Label(new Rect(0, 0, 100, 100), "FPS: " + ((int)(1.0f / Time.smoothDeltaTime)).ToString());
            GUI.Label(new Rect(0, 20, 200, 100), "Vehicle Count : " + spawnedVeh.ToString());
            GUI.Label(new Rect(0, 40, 200, 100), "Detected Spawn Nodes : " + nodeCount.ToString());
        }
    }
    // Update is called once per frame
    //void FixedUpdate()
    //{
    //spawnFrame += 1;
    //if (spawnFrame == skipFrames)
    //{
    //    if (spawnedVeh < vehicleQuantity)
    //    {
    //        Collider[] cols = Physics.OverlapSphere(transform.position, maxVehSpawnRadius, layerMask);
    //        for (int i = 0; i < cols.Length; i++)
    //        {
    //            int choice = Random.Range(0, cols.Length);
    //            Collider col = cols[choice];
    //            if ((col.transform.position - transform.position).sqrMagnitude > minVehSpawnRadius * minVehSpawnRadius)
    //            {
    //                //if (Random.value > 0.2f) continue;
    //                Node n = col.GetComponent<Node>();
    //                float overlapRadius = 8;// + n.speedLimit*0.1f;
    //                int vehDens = n.spawnDensity;
    //                if (Physics.OverlapSphere(n.transform.position, 100, vehicleLayer).Length <= vehDens)//density check
    //                {
    //                    if (n != null && Physics.OverlapSphere(n.transform.position, overlapRadius, vehicleLayer).Length == 0 && (n.branches.Count > 0 || n.nodeType == Node.NodeType.parking))
    //                    {
    //                        GameObject ob = ObjectPooler.instance.SpawnVehFromPool("Vehicles", n.transform.position, Quaternion.identity);
    //                        if (ob != null)
    //                        {
    //                            ob.GetComponent<VehicleController>().SetInitialTransform(n);
    //                            spawnedVeh += 1;
    //                        }
    //                        break;
    //                    }
    //                }
    //            }
    //        }
    //    }
    //    spawnFrame = 0;
    //}
    //}

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            if (spawnedVeh < vehicleQuantity)
            {
                Collider[] cols = Physics.OverlapSphere(transform.position, maxVehSpawnRadius, layerMask);
                for (int i = 0; i < 10; i++)
                {
                    if (cols.Length <= 0)
                    {
                        yield return new WaitForFixedUpdate();
                        yield return new WaitForFixedUpdate();
                        break;
                    }
                    int choice = Random.Range(0, cols.Length);
                    Collider col = cols[choice];
                    nodeCount = cols.Length;
                    if ((col.transform.position - transform.position).sqrMagnitude > minVehSpawnRadius * minVehSpawnRadius)
                    {
                        //if (Random.value > 0.2f) continue;
                        Node n = col.GetComponent<Node>();
                        float overlapRadius = 4;// + n.speedLimit*0.1f;
                        int vehDens = n.spawnDensity;
                        if (Physics.OverlapSphere(n.transform.position, 100, vehicleLayer).Length <= vehDens)//density check
                        {
                            yield return new WaitForFixedUpdate();
                            if (n != null && Physics.OverlapSphere(n.transform.position, overlapRadius, vehicleLayer).Length == 0 && (n.branches.Count > 0 || n.nodeType == Node.NodeType.parking))
                            {
                                GameObject ob = ObjectPooler.instance.SpawnVehFromPool("Vehicles", n.transform.position, Quaternion.identity);
                                if (ob != null)
                                {
                                    ob.GetComponent<VehicleController>().SetInitialTransform(n);
                                    spawnedVeh += 1;
                                }
                                break;
                            }
                        }
                    }
                }
                yield return new WaitForFixedUpdate();
            }
            yield return new WaitForFixedUpdate();
        }
    }
}

[Serializable]
public class VehiclePopulation
{
    public GameObject prefab;
    public int amount;
}

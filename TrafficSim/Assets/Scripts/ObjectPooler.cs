using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    public static ObjectPooler instance;

    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
    }
    public Dictionary<string, Queue<GameObject>> poolDictionary;
    public Dictionary<string, List<GameObject>> vehPoolDictionary;

    public List<Pool> pools;

    private void Awake()
    {
        if (instance != null) Destroy(this.gameObject);
        else
            instance = this;
        poolDictionary = new Dictionary<string, Queue<GameObject>>();
        vehPoolDictionary = new Dictionary<string, List<GameObject>>();
    }

    public void InitializePool(string tag, GameObject[] objs)
    {
        Queue<GameObject> objectPool = new Queue<GameObject>();
        for (int i = 0; i < objs.Length; i++)
        {
            GameObject ob = Instantiate(objs[i]);
            ob.SetActive(false);
            objectPool.Enqueue(ob);
        }
        if (poolDictionary.ContainsKey(tag))
        {
            foreach (GameObject obj in poolDictionary[tag])
            {
                Destroy(obj);
            }
            poolDictionary.Remove(tag);
        }
        poolDictionary.Add(tag, objectPool);
    }

    public void InitializePool(string tag, GameObject prefab, int size)
    {
        Queue<GameObject> objectPool = new Queue<GameObject>();
        for (int i = 0; i < size; i++)
        {
            GameObject ob = Instantiate(prefab);
            ob.SetActive(false);
            objectPool.Enqueue(ob);
        }
        if (poolDictionary.ContainsKey(tag))
        {
            foreach (GameObject obj in poolDictionary[tag])
            {
                Destroy(obj);
            }
            poolDictionary.Remove(tag);
        }
        poolDictionary.Add(tag, objectPool);
    }

    public void InitializeVehPool(string tag, GameObject prefab, int size)
    {
        List<GameObject> objectPool = new List<GameObject>();
        for (int i = 0; i < size; i++)
        {
            GameObject ob = Instantiate(prefab);
            ob.SetActive(false);
            objectPool.Add(ob);
        }
        if (vehPoolDictionary.ContainsKey(tag))
        {
            foreach (GameObject obj in vehPoolDictionary[tag])
            {
                Destroy(obj);
            }
            vehPoolDictionary.Remove(tag);
        }
        vehPoolDictionary.Add(tag, objectPool);
    }

    public void InitializeVehPool(string tag, GameObject[] vehicles)
    {
        List<GameObject> objectPool = new List<GameObject>();
        for (int i = 0; i < vehicles.Length; i++)
        {
            GameObject ob = Instantiate(vehicles[i]);
            ob.SetActive(false);
            objectPool.Add(ob);
        }
        if (vehPoolDictionary.ContainsKey(tag))
        {
            foreach (GameObject obj in vehPoolDictionary[tag])
            {
                Destroy(obj);
            }
            vehPoolDictionary.Remove(tag);
        }
        vehPoolDictionary.Add(tag, objectPool);
    }



    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogError("Tag: " + tag + " not found in pool dictionary");
            return null;
        }
        GameObject spawnObj = poolDictionary[tag].Dequeue();

        spawnObj.SetActive(true);
        spawnObj.transform.position = position;
        spawnObj.transform.rotation = rotation;

        IPoolObject poolObj = spawnObj.GetComponent<IPoolObject>();
        if (poolObj != null)
        {
            poolObj.OnSpawn();
        }

        poolDictionary[tag].Enqueue(spawnObj);

        return spawnObj;
    }

    public GameObject SpawnVehFromPool(string tag,Vector3 position,Quaternion rotation)
    {
        if (!vehPoolDictionary.ContainsKey(tag))
        {
            Debug.LogError("Tag: " + tag + " not found in pool dictionary");
            return null;
        }
        if (vehPoolDictionary[tag].Count == 0)
        {
            Debug.LogError("vehicle prefab pool is empty");
            return null;
        }
        int c = Random.Range(0, vehPoolDictionary[tag].Count);
        GameObject veh = vehPoolDictionary[tag][c];
        vehPoolDictionary[tag].Remove(veh);

        veh.SetActive(true);
        veh.transform.position = position;
        veh.transform.rotation = rotation;

        IPoolObject poolObj = veh.GetComponent<IPoolObject>();
        if (poolObj != null)
        {
            poolObj.OnSpawn();
        }

        return veh;
    }
    
    public void StoreVehInPool(string tag,GameObject ob)
    {
        if (!vehPoolDictionary.ContainsKey(tag))
        {
            Debug.LogError("Tag: " + tag + " not found in pool dictionary");
            return;
        }
        ob.SetActive(false);
        vehPoolDictionary[tag].Add(ob);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public Transform splatHolder;
    public SpawnManager spawnManager;
    public ObjectPoolManager objectPools;

    public int createdPooledObjects;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);

        spawnManager = GetComponentInChildren<SpawnManager>();
        objectPools = GetComponentInChildren<ObjectPoolManager>();

    }

    public static void AddCreations()
    {
        Instance.createdPooledObjects++;
    }

    public static PooledObject GetPooledObject(ObjectPoolManager.PoolRequestInfo info)
    {
        return Instance.objectPools.GetObject(info);
    }

    public static void ReturnPooledObject(PooledObject obj)
    {
        Instance.objectPools.ReturnObject(obj);
    }

}

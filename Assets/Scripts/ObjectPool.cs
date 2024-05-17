using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public GameObject prefab;
    public int poolSize = 30;

    private Queue<GameObject> pool = new Queue<GameObject>();

    private void Start()
    {
        InitializePool();
    }

    private void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = new GameObject();
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    public GameObject GetObject()
    {
        if (pool.Count == 0)
        {
            Debug.LogWarning("Pool is empty. Returning null.");
            return null;
        }

        GameObject obj = pool.Dequeue();
        obj.SetActive(true);
        //Debug.Log($"GetObject()! pool count:{pool.Count}");
        return obj;
    }

    public void ReturnObject(GameObject obj)
    {
        obj.SetActive(false);
        pool.Enqueue(obj);
        //Debug.Log($"ReturnObject()! pool count:{pool.Count}");
    }
}


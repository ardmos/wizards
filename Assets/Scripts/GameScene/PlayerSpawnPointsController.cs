using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawnPointsController : MonoBehaviour
{
    public Transform[] spawnPoints;

    [SerializeField] private int spawnIndex = 0;
    
    // Start is called before the first frame update
    void Start()
    {
        foreach (var spawnPoint in spawnPoints)
        {
            spawnPoint.GetComponent<MeshRenderer>().enabled = false;
        }
    }

    /*    public Vector3 GetSpawnPoint(int index)
        {
            return spawnPoints[index].position;
        }*/

    public Vector3 GetSpawnPoint()
    {
        Debug.Log($"GetSpawnPoint() spawnIndex:{spawnIndex} requested!");
        Vector3 spawnPosition = spawnPoints[spawnIndex].position;
        spawnIndex++; // 순환하도록 인덱스를 증가시킴
        return spawnPosition;
    }
}

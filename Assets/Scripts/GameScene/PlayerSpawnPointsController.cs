using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawnPointsController : MonoBehaviour
{
    public Transform[] spawnPoints;
    
    // Start is called before the first frame update
    void Start()
    {
        foreach (var spawnPoint in spawnPoints)
        {
            spawnPoint.GetComponent<MeshRenderer>().enabled = false;
        }
    }

    public Vector3 GetSpawnPoint(int index)
    {
        return spawnPoints[index].position;
    }
}

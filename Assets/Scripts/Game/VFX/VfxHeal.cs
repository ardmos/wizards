using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class VfxHeal : NetworkBehaviour
{
    void Start()
    {
        if (!IsServer) return;
        
        StartCoroutine(DespawnThis());
    }

    private IEnumerator DespawnThis()
    {
        yield return new WaitForSeconds(2f);
        GetComponent<NetworkObject>().Despawn();
    }
}

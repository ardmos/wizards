using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class VfxHeal : NetworkBehaviour
{
    [SerializeField] private float lifeTime = 1.8f;
    void Start()
    {
        if (!IsServer) return;
        
        StartCoroutine(DespawnThis());
    }

    private IEnumerator DespawnThis()
    {
        yield return new WaitForSeconds(lifeTime);
        GetComponent<NetworkObject>().Despawn();
    }
}

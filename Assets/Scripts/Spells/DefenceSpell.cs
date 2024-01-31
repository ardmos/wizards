using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public abstract class DefenceSpell : NetworkBehaviour
{
    [SerializeField] protected SpellInfo spellInfo;

    public virtual void InitSpellInfoDetail(SpellInfo spellInfoFromServer)
    {
        if (!IsServer) return;

        spellInfo = spellInfoFromServer;
        Debug.Log($"InitSpellInfoDetail! MagicShield Lv1 spell lifeTime: {spellInfo.lifeTime}");
    }

    public virtual void Activate() 
    {
        if (!IsServer) return;

        Debug.Log($"MagicShield Lv1 Activate");
        Debug.Log($"MagicShield Lv1 spell lifeTime: {spellInfo.lifeTime}");
        Destroy(gameObject, spellInfo.lifeTime);
    }

    /// <summary>
    /// 2. CollisionEnter �浹 ó�� (���� ���� ���)
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter(Collision collision)
    {
        // ���������� ó��.
        if (!IsServer) return;
        if (!collision.transform.CompareTag("Attack")) return;
        Debug.Log($"������ ���� �浹!! name : {collision.gameObject.name}");
    }
}

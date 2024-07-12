using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Matchmaker.Models;
using UnityEngine;

public abstract class AoESpell : NetworkBehaviour, IOwnerSeter
{
    [SerializeField] protected SpellInfo spellInfo;

    [SerializeField] private ulong _shooterClientID;

    [Header("스킬의 영향을 받는 플레이어 목록")]
    [SerializeField] protected List<GameObject> playersInArea = new List<GameObject>();

    [Header("AI가 피격됐을 시 타겟으로 설정될 마법을 소유한 플레이어 오브젝트.")]
    public GameObject spellOwnerObject = null;

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;
        // 시전자는 영향 안받도록 설정
        if (other.gameObject == spellOwnerObject || spellOwnerObject == null) return;

        if (other.CompareTag("Player"))
        {
            플레이어전용트리거엔터(other.gameObject);
        }
        else if (other.CompareTag("AI"))
        {
            AI전용트리거엔터(other.gameObject);
        }
        else if (other.CompareTag("Monster"))
        {
            Monster전용트리거엔터(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsServer) return;
        // 시전자는 영향 안받도록 설정
        if (other.gameObject == spellOwnerObject || spellOwnerObject == null) return;

        if (other.CompareTag("Player"))
        {
            플레이어전용트리거엑싯(other.gameObject);
        }
        else if (other.CompareTag("AI"))
        {
            AI전용트리거엑싯(other.gameObject);
        }
        else if (other.CompareTag("Monster"))
        {
            Monster전용트리거엑싯(other.gameObject);
        }
    }

    public void SetOwner(ulong shooterClientID, GameObject spellOwnerObject)
    {
        //Debug.Log($"AoESpell.SetOwner메서드. 요청자:{shooterClientID}, {spellOwnerObject} // 서버인가?:{IsServer}");
        if (!IsServer) return;
        _shooterClientID = shooterClientID;
        this.spellOwnerObject = spellOwnerObject;

        //Debug.Log($"AI 플레이어{shooterClientID}");
    }

    public virtual void InitAoESpell(SpellInfo spellInfoFromServer)
    {
        if (IsClient) return;

        spellInfo = spellInfoFromServer;
    }

    protected abstract void 플레이어전용트리거엔터(GameObject gameObject);
    protected abstract void AI전용트리거엔터(GameObject gameObject);
    protected abstract void Monster전용트리거엔터(GameObject gameObject);

    protected abstract void 플레이어전용트리거엑싯(GameObject gameObject);
    protected abstract void AI전용트리거엑싯(GameObject gameObject);
    protected abstract void Monster전용트리거엑싯(GameObject gameObject);
}

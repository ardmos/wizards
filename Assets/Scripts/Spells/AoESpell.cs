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

    [Header("��ų�� ������ �޴� �÷��̾� ���")]
    [SerializeField] protected List<GameObject> playersInArea = new List<GameObject>();

    [Header("AI�� �ǰݵ��� �� Ÿ������ ������ ������ ������ �÷��̾� ������Ʈ.")]
    public GameObject spellOwnerObject = null;

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;
        // �����ڴ� ���� �ȹ޵��� ����
        if (other.gameObject == spellOwnerObject || spellOwnerObject == null) return;

        if (other.CompareTag("Player"))
        {
            �÷��̾�����Ʈ���ſ���(other.gameObject);
        }
        else if (other.CompareTag("AI"))
        {
            AI����Ʈ���ſ���(other.gameObject);
        }
        else if (other.CompareTag("Monster"))
        {
            Monster����Ʈ���ſ���(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsServer) return;
        // �����ڴ� ���� �ȹ޵��� ����
        if (other.gameObject == spellOwnerObject || spellOwnerObject == null) return;

        if (other.CompareTag("Player"))
        {
            �÷��̾�����Ʈ���ſ���(other.gameObject);
        }
        else if (other.CompareTag("AI"))
        {
            AI����Ʈ���ſ���(other.gameObject);
        }
        else if (other.CompareTag("Monster"))
        {
            Monster����Ʈ���ſ���(other.gameObject);
        }
    }

    public void SetOwner(ulong shooterClientID, GameObject spellOwnerObject)
    {
        //Debug.Log($"AoESpell.SetOwner�޼���. ��û��:{shooterClientID}, {spellOwnerObject} // �����ΰ�?:{IsServer}");
        if (!IsServer) return;
        _shooterClientID = shooterClientID;
        this.spellOwnerObject = spellOwnerObject;

        //Debug.Log($"AI �÷��̾�{shooterClientID}");
    }

    public virtual void InitAoESpell(SpellInfo spellInfoFromServer)
    {
        if (IsClient) return;

        spellInfo = spellInfoFromServer;
    }

    protected abstract void �÷��̾�����Ʈ���ſ���(GameObject gameObject);
    protected abstract void AI����Ʈ���ſ���(GameObject gameObject);
    protected abstract void Monster����Ʈ���ſ���(GameObject gameObject);

    protected abstract void �÷��̾�����Ʈ���ſ���(GameObject gameObject);
    protected abstract void AI����Ʈ���ſ���(GameObject gameObject);
    protected abstract void Monster����Ʈ���ſ���(GameObject gameObject);
}

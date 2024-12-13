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

        spellInfo = new SpellInfo(spellInfoFromServer);
    }

    public SpellInfo GetSpellInfo()
    {
        return spellInfo;
    }

    public virtual void Activate() 
    {
        if (!IsServer) return;

        Destroy(gameObject, spellInfo.lifetime);
        //StartCoroutine(StartCountdown(spellInfo.lifeTime));
    }

    public void PlaySFX(SFX_Type sFX_Type)
    {
        SoundManager.Instance?.PlayWizardSpellSFX(spellInfo.spellName, sFX_Type, transform);
    }

    /// <summary>
    /// CollisionEnter �浹 ó�� (���� ���� ���)
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServer) return;
        if (!collision.transform.CompareTag("Attack")) return;

        Debug.Log($"������ ���� �浹!! name : {collision.gameObject.name}");
    }
}

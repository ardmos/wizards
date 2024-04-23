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

        Destroy(gameObject, spellInfo.lifeTime);
        //StartCoroutine(StartCountdown(spellInfo.lifeTime));
    }
/*    /// <summary>
    /// lifeTime �ڿ� ������Ʈ �ı� �� �÷��̾� �ִϸ��̼� ������Ʈ ����
    /// </summary>
    private IEnumerator StartCountdown(float lifeTime)
    {
        yield return new WaitForSeconds(lifeTime);

        // �÷��̾� ĳ���Ͱ� Casting �ִϸ��̼����� �ƴ� ��쿡�� Idle�� ����
        if (!GameMultiplayer.Instance.GetPlayerDataFromClientId(spellInfo.ownerPlayerClientId).playerAttackAnimState.Equals(PlayerAttackAnimState.CastingAttackMagic))
        {
            GameMultiplayer.Instance.UpdatePlayerAttackAnimStateOnServer(spellInfo.ownerPlayerClientId, PlayerAttackAnimState.Idle);
        }

        Destroy(gameObject);
    }*/

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

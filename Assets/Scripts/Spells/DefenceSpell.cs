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
    /// lifeTime 뒤에 오브젝트 파괴 및 플레이어 애니메이션 업데이트 보고
    /// </summary>
    private IEnumerator StartCountdown(float lifeTime)
    {
        yield return new WaitForSeconds(lifeTime);

        // 플레이어 캐릭터가 Casting 애니메이션중이 아닐 경우에만 Idle로 변경
        if (!GameMultiplayer.Instance.GetPlayerDataFromClientId(spellInfo.ownerPlayerClientId).playerAttackAnimState.Equals(PlayerAttackAnimState.CastingAttackMagic))
        {
            GameMultiplayer.Instance.UpdatePlayerAttackAnimStateOnServer(spellInfo.ownerPlayerClientId, PlayerAttackAnimState.Idle);
        }

        Destroy(gameObject);
    }*/

    /// <summary>
    /// CollisionEnter 충돌 처리 (서버 권한 방식)
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServer) return;
        if (!collision.transform.CompareTag("Attack")) return;

        Debug.Log($"방어마법에 공격 충돌!! name : {collision.gameObject.name}");
    }
}

using Unity.Netcode;
using UnityEngine;
/// <summary>
/// 
/// 1레벨 파이어볼 스크립트입니다.
/// 
/// !!! 현재 기능
/// 1. 상세 능력치 설정
/// </summary>
public class FireBallLv1 : FireSpell
{
    // 현재 사용하는 파이어볼 VFX를 자연스럽게 하기위한 부분
    public override void OnNetworkSpawn()
    {
        trails[0].SetActive(false);
    }

    public override void Shoot(Vector3 force, ForceMode forceMode)
    {
        base.Shoot(force, forceMode);

        // 꼬리연기 효과 활성화
        trails[0].SetActive(true);
    }

    /// <summary>
    /// 2. CollisionEnter 충돌 처리 (서버 권한 방식)
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter(Collision collision)
    {
        // 서버에서만 처리.
        if (IsClient) return;

        //Debug.Log($"마법 충돌! 마법:{gameObject.name}, 충돌체:{collision.gameObject.name}");

        // 충돌을 중복 처리하는것을 방지하기 위한 처리
        GetComponent<Collider>().enabled = false;
        ulong spellOwnerClientId = GetSpellInfo().ownerPlayerClientId;

        // 범위딜로 만들기 위한 처리
        // 충돌한 객체가 Player 또는 AI 태그를 가지고 있는지 확인
        if (collision.collider.CompareTag("Player") || collision.collider.CompareTag("AI"))
        {
            // 충돌 지점
            Vector3 explosionPosition = collision.contacts[0].point;
            // 범위 3 내의 모든 객체 찾기
            Collider[] colliders = Physics.OverlapSphere(explosionPosition, 3f);

            foreach (Collider hit in colliders)
            {
                // 데미지 입히기

                // 충돌한게 공격마법이면서 파이어볼 본체와 충돌한 경우, 어떤 마법이 살아남을지 계산에 들어감
                if (hit.gameObject == collision.gameObject && hit.CompareTag("AttackSpell"))
                {
                    SpellHitHandlerOnServer(hit);
                }
                else if (hit.CompareTag("AttackSkill"))
                {

                }
                // 충돌한게 플레이어일 경우, 플레이어의 피격 사실을 해당 플레이어의 SpellManager 알립니다. 
                else if (hit.CompareTag("Player"))
                {
                    // 시전자는 피해 안받도록 설정
                    if (hit.gameObject.layer == shooterLayer) continue;

                    if (GetSpellInfo() == null) return;

                    if (hit.TryGetComponent<PlayerServer>(out PlayerServer playerServer))
                    {
                        sbyte damage = (sbyte)GetSpellInfo().level;
                        // 플레이어 피격을 서버에서 처리
                        playerServer.PlayerGotHitOnServer(damage, spellOwnerClientId);
                    }
                }
                // AI플레이어일 경우 처리
                else if (hit.CompareTag("AI"))
                {
                    // 시전자는 피해 안받도록 설정
                    if (hit.gameObject.layer == shooterLayer) continue;

                    if (GetSpellInfo() == null) return;

                    // WizardRukeAI 확인.  추후 다른 AI추가 후 수정.         
                    if (hit.TryGetComponent<WizardRukeAIServer>(out WizardRukeAIServer aiPlayer))
                    {
                        sbyte damage = (sbyte)GetSpellInfo().level;
                        // 플레이어 피격을 서버에서 처리
                        aiPlayer.PlayerGotHitOnServer(damage, spellOwnerClientId, spellOwnerObject);
                    }
                }
                // 기타 오브젝트 충돌
                else
                {
                    //Debug.Log($"{collider.name} Hit!");
                }
            }
        }

        // 혹시 시전자가 Casting 상태였으면 Cooltime 상태로 넘겨주기 
        // 시전자가 AI일 경우
        if (spellOwnerObject.TryGetComponent<WizardRukeAISpellManagerServer>(out WizardRukeAISpellManagerServer wizardRukeAISpellManagerServer))
        {
            if (wizardRukeAISpellManagerServer.GetSpellInfo(GetSpellInfo().spellName).spellState == SpellState.Aiming)
            {
                //Debug.Log($"캐스팅 상태에서 폭발했습니다!!! 스킬 상태를 종료합니다. spellState:{spellManagerServerWizard.GetSpellInfo(GetSpellInfo().spellName).spellState} -> ");
                int spellIndex = wizardRukeAISpellManagerServer.GetSpellIndexBySpellName(GetSpellInfo().spellName);
                if (spellIndex != -1)
                {
                    wizardRukeAISpellManagerServer.UpdatePlayerSpellState((ushort)spellIndex, SpellState.Cooltime);
                    wizardRukeAISpellManagerServer.playerAnimator.UpdateWizardMaleAnimationOnServer(WizardMaleAnimState.ShootingMagic);
                    //Debug.Log($"{spellManagerServerWizard.GetSpellInfo(GetSpellInfo().spellName).spellState}.");
                }
                else Debug.Log($"스펠 인덱스를 찾지 못했습니다.");
            }
        }
        // 시전자가 플레이어일 경우
        else if(spellOwnerObject.TryGetComponent<SpellManagerServerWizard>(out SpellManagerServerWizard spellManagerServerWizard))
        {
            if (spellManagerServerWizard.GetSpellInfo(GetSpellInfo().spellName).spellState == SpellState.Aiming)
            {
                //Debug.Log($"캐스팅 상태에서 폭발했습니다!!! 스킬 상태를 종료합니다. spellState:{spellManagerServerWizard.GetSpellInfo(GetSpellInfo().spellName).spellState} -> ");
                int spellIndex = spellManagerServerWizard.GetSpellIndexBySpellName(GetSpellInfo().spellName);
                if (spellIndex != -1)
                {
                    spellManagerServerWizard.UpdatePlayerSpellState((ushort)spellIndex, SpellState.Cooltime);
                    spellManagerServerWizard.playerAnimator.UpdateWizardMaleAnimationOnServer(WizardMaleAnimState.ShootingMagic);
                    //Debug.Log($"{spellManagerServerWizard.GetSpellInfo(GetSpellInfo().spellName).spellState}.");
                }
                else Debug.Log($"스펠 인덱스를 찾지 못했습니다.");
            }
        }

        // 마법 충돌 사운드 재생
        SoundManager.Instance?.PlayWizardSpellSFX(spellInfo.spellName, SFX_Type.Hit, transform);

        // 적중 효과 VFX
        HitVFX(GetHitVFXPrefab(), collision);

        Destroy(gameObject, 0.2f);
    }
}

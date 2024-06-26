using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Matchmaker.Models;
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
    [Header("파이어볼용 변수들")]
    public SphereCollider sphereCollider;
    public float explosionRadius;
    public float piercingStack;
    public sbyte damagePerSecond;
    public float duration;
    public Transform beam;
    public Transform ball;
    public Transform smoke;

    public override void OnNetworkSpawn()
    {
        // 현재 사용하는 파이어볼 VFX를 자연스럽게 하기위한 부분
        trails[0].SetActive(false);
    }

    public override void Shoot(Vector3 force, ForceMode forceMode)
    {
        base.Shoot(force, forceMode);

        // 꼬리연기 효과 활성화
        trails[0].SetActive(true);
    }

    public override void InitSpellInfoDetail(SpellInfo spellInfoFromServer, GameObject spellOwnerObject)
    {
        base.InitSpellInfoDetail(spellInfoFromServer, spellOwnerObject);

        explosionRadius = 3f;
        piercingStack = 0f;
        damagePerSecond = 0;
        duration = 2f;
        // 업그레이드 현황 적용
        업그레이드현황적용();       
    }

    /// <summary>
    /// 업그레이드 현황을 적용합니다
    /// </summary>
    private void 업그레이드현황적용()
    {
        foreach (FireballUpgradeOption upgradeOption in System.Enum.GetValues(typeof(FireballUpgradeOption)))
        {
            if (spellInfo.upgradeOptions[(int)upgradeOption] != 0)
            {
                switch (upgradeOption)
                {
                    case FireballUpgradeOption.IncreaseSize:
                        // "파이어볼의 크기가 10% 증가합니다."
                        beam.localScale *= 1f + spellInfo.upgradeOptions[(int)upgradeOption]*0.1f;
                        ball.localScale *= 1f + spellInfo.upgradeOptions[(int)upgradeOption] * 0.1f;
                        smoke.localScale *= 1f + spellInfo.upgradeOptions[(int)upgradeOption] * 0.1f;
                        sphereCollider.radius *= 1f + spellInfo.upgradeOptions[(int)upgradeOption] * 0.1f;
                        break;
                    case FireballUpgradeOption.AddDotDamage:
                        // "파이어볼에 맞은 적이 5초 동안 초당 1의 화염 피해를 입습니다."
                        damagePerSecond += (sbyte)spellInfo.upgradeOptions[(int)upgradeOption];
                        break;
                    case FireballUpgradeOption.IncreaseExplosionRadius:
                        // "파이어볼의 폭발 범위를 1 증가시킵니다."
                        explosionRadius += spellInfo.upgradeOptions[(int)upgradeOption];
                        break;
                    case FireballUpgradeOption.ReduceCooldown:
                        // "파이어볼의 재사용 대기 시간이 20% 감소합니다."
                        float defaultCooltime = SpellSpecifications.Instance.GetSpellDefaultSpec(SkillName.FireBallLv1).coolTime;
                        float reductionPercentage = 0.2f * (float)spellInfo.upgradeOptions[(int)upgradeOption];
                        spellInfo.coolTime = defaultCooltime * (1 - reductionPercentage);
                        //Debug.Log($"{SpellSpecifications.Instance.GetSpellDefaultSpec(SkillName.FireBallLv1).coolTime} - 0.2f * {spellInfo.upgradeOptions[(int)upgradeOption]}. 파이어볼 쿨타임:{spellInfo.coolTime}");
                        break;
                    case FireballUpgradeOption.AddPiercing:
                        // "파이어볼이 적을 관통합니다."
                        piercingStack += spellInfo.upgradeOptions[(int)upgradeOption];
                        break;
                }

            }
        }
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

        // 범위딜로 만들기 위한 처리
        범위딜처리(collision);

        // 혹시 시전자가 Casting 상태였으면 Cooltime 상태로 넘겨주기 
        현스킬이캐스팅상태였을경우();

        // 마법 충돌 사운드 재생
        SoundManager.Instance?.PlayWizardSpellSFX(spellInfo.spellName, SFX_Type.Hit, transform);

        // 적중 효과 VFX
        HitVFX(GetHitVFXPrefab(), collision);

        if (!피어싱효과구현()) return;

        공격마법과충돌한경우(collision);

        Destroy(gameObject, 0.2f);
    }

    private void 범위딜처리(Collision collision)
    {
        ulong spellOwnerClientId = GetSpellInfo().ownerPlayerClientId;
        // 충돌 지점
        Vector3 explosionPosition = collision.contacts[0].point;
        Collider[] colliders = Physics.OverlapSphere(explosionPosition, explosionRadius);
        // 범위 내 충돌체 인식
        foreach (Collider hit in colliders)
        {
            // 시전자는 피해 안받도록 설정
            if (hit.gameObject == spellOwnerObject) continue;

            // 충돌한게 플레이어일 경우, 플레이어의 피격 사실을 해당 플레이어의 SpellManager 알립니다. 
            if (hit.CompareTag("Player"))
            {
                if (GetSpellInfo() == null) return;

                if (hit.TryGetComponent<PlayerServer>(out PlayerServer playerServer))
                {
                    sbyte damage = (sbyte)GetSpellInfo().damage;
                    // 플레이어 피격을 서버에서 처리
                    playerServer.PlayerGotHitOnServer(damage, spellOwnerClientId);

                    // 도트 데미지 실행
                    if (damagePerSecond > 0)
                        playerServer.StartCoroutine(playerServer.TakeDamageOverTime(damagePerSecond, duration, spellOwnerClientId));
                }
            }
            // AI플레이어일 경우 처리
            else if (hit.CompareTag("AI"))
            {
                if (GetSpellInfo() == null) return;

                // WizardRukeAI 확인.  추후 다른 AI추가 후 수정.         
                if (hit.TryGetComponent<WizardRukeAIServer>(out WizardRukeAIServer aiPlayer))
                {
                    sbyte damage = (sbyte)GetSpellInfo().damage;
                    // 플레이어 피격을 서버에서 처리
                    aiPlayer.PlayerGotHitOnServer(damage, spellOwnerClientId, spellOwnerObject);

                    // 도트 데미지 실행
                    if (damagePerSecond > 0)
                        aiPlayer.StartCoroutine(aiPlayer.TakeDamageOverTime(damagePerSecond, duration, spellOwnerClientId));
                }
            }
            // 기타 오브젝트 충돌
            else
            {
                //Debug.Log($"{collider.name} Hit!");
            }
        }
    }

    private void 현스킬이캐스팅상태였을경우()
    {
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
        else if (spellOwnerObject.TryGetComponent<SpellManagerServerWizard>(out SpellManagerServerWizard spellManagerServerWizard))
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

    }

    private bool 피어싱효과구현()
    {
        // 피어싱 스택이 남아있으면 스펠파괴 로직 실행 대신, 피어싱 스택을 감소시킵니다
        if (--piercingStack >= 0)
        {
            Debug.Log($"피어싱 스택 감소!{piercingStack}");
            return true;
        }
        return false;
    }

    private void 공격마법과충돌한경우(Collision collision)
    {
        // 충돌한게 공격마법이면서 파이어볼 본체와 충돌한 경우, 어떤 마법이 살아남을지 계산에 들어감
        if (collision.collider.CompareTag("AttackSpell"))
        {
            SpellHitHandlerOnServer(collision.collider);
        }
        else if (collision.collider.CompareTag("AttackSkill"))
        {

        }
    }
}

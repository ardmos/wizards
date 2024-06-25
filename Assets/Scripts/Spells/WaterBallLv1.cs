using System.Collections;
using Unity.Netcode;
using Unity.Services.Matchmaker.Models;
using UnityEngine;
/// <summary>
/// 1레벨 워터볼 스크립트입니다.
/// </summary>
public class WaterBallLv1 : WaterSpell
{
    [Header("워터볼용 변수들")]
    public HomingMissile homingMissile;
    public bool isSplashON;
    public float explosionRadius = 3f;

    public override void InitSpellInfoDetail(SpellInfo spellInfoFromServer, GameObject spellOwnerObject)
    {
        base.InitSpellInfoDetail(spellInfoFromServer, spellOwnerObject);

        isSplashON = false;

        // 업그레이드 현황 적용
        업그레이드현황적용();
    }

    /// <summary>
    /// 업그레이드 현황을 적용합니다
    /// </summary>
    private void 업그레이드현황적용()
    {
        foreach (WaterballUpgradeOption upgradeOption in System.Enum.GetValues(typeof(WaterballUpgradeOption)))
        {
            if (spellInfo.upgradeOptions[(int)upgradeOption] != 0)
            {
                switch (upgradeOption)
                {
                    case WaterballUpgradeOption.IncreaseSpeed:
                        // "워터볼의 속도가 30% 증가합니다."
                        spellInfo.moveSpeed = SpellSpecifications.Instance.GetSpellDefaultSpec(SkillName.WaterBallLv1).moveSpeed - 0.2f * (float)spellInfo.upgradeOptions[(int)upgradeOption];
                        break;
                    case WaterballUpgradeOption.IncreaseHomingRange:
                        // "워터볼의 유도 타겟 인식 범위가 20% 증가합니다."
                        float defaultRange = homingMissile.GetMaxHomingRange();
                        float increasePercentage = 0.2f * (float)spellInfo.upgradeOptions[(int)upgradeOption];
                        homingMissile.SetMaxHomingRange(defaultRange * (1 + increasePercentage));
                        //damagePerSecond += (sbyte)spellInfo.upgradeOptions[(int)upgradeOption];
                        break;
                    case WaterballUpgradeOption.AddSplashDamage:
                        // "워터볼이 적중 시 주변에 범위 피해를 입힙니다."
                        isSplashON = true;
                        break;
                    case WaterballUpgradeOption.ReduceCooldown:
                        // "워터볼의 재사용 대기 시간이 20% 감소합니다."
                        float defaultCooltime = SpellSpecifications.Instance.GetSpellDefaultSpec(SkillName.FireBallLv1).coolTime;
                        float reductionPercentage = 0.2f * (float)spellInfo.upgradeOptions[(int)upgradeOption];
                        spellInfo.coolTime = defaultCooltime * (1 - reductionPercentage);
                        //Debug.Log($"{SpellSpecifications.Instance.GetSpellDefaultSpec(SkillName.FireBallLv1).coolTime} - 0.2f * {spellInfo.upgradeOptions[(int)upgradeOption]}. 파이어볼 쿨타임:{spellInfo.coolTime}");
                        break;
                    case WaterballUpgradeOption.IncreaseRange:
                        // "워터볼의 사거리가 50% 증가합니다."
                        float defaultLifetime = spellInfo.lifeTime;
                        float increaseLifetime = defaultLifetime * 0.5f * spellInfo.upgradeOptions[(int)upgradeOption];
                        DestroyAfterDelay(defaultLifetime + increaseLifetime); 
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

        if (isSplashON)
        {
            범위딜처리(collision);
        }
        else
        {
            단일딜처리(collision);
        }

        // 혹시 시전자가 Casting 상태였으면 Cooltime 상태로 넘겨주기 
        현스킬이캐스팅상태였을경우();

        // 마법 충돌 사운드 재생
        SoundManager.Instance?.PlayWizardSpellSFX(spellInfo.spellName, SFX_Type.Hit, transform);

        // 적중 효과 VFX
        HitVFX(GetHitVFXPrefab(), collision);

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
            // 충돌한게 플레이어일 경우, 플레이어의 피격 사실을 해당 플레이어의 SpellManager 알립니다. 
            if (hit.CompareTag("Player"))
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

    private void 단일딜처리(Collision collision)
    {
        Collider collider = collision.collider;
        ulong spellOwnerClientId = GetSpellInfo().ownerPlayerClientId;

        // 충돌한게 플레이어일 경우, 플레이어의 피격 사실을 해당 플레이어의 SpellManager 알립니다. 
        if (collider.CompareTag("Player"))
        {
            if (GetSpellInfo() == null)
            {
                //Debug.Log("AttackSpell Info is null");
                return;
            }

            PlayerServer player = collider.GetComponent<PlayerServer>();
            if (player == null)
            {
                //Debug.LogError("Player is null!");
                return;
            }

            sbyte damage = (sbyte)GetSpellInfo().level;
            // 플레이어 피격을 서버에서 처리
            player.PlayerGotHitOnServer(damage, spellOwnerClientId);
        }
        // AI플레이어일 경우 처리
        else if (collider.CompareTag("AI"))
        {
            if (GetSpellInfo() == null) return;

            // WizardRukeAI 확인.  추후 다른 AI추가 후 수정.         
            if (collider.TryGetComponent<WizardRukeAIServer>(out WizardRukeAIServer aiPlayer))
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

    private void 공격마법과충돌한경우(Collision collision)
    {
        // 충돌한게 공격마법일 경우, 어떤 마법이 살아남을지 계산에 들어감
        if (collision.collider.CompareTag("AttackSpell"))
        {
            SpellHitHandlerOnServer(collision.collider);
        }
        else if (collision.collider.CompareTag("AttackSkill"))
        {

        }
    }

    private IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // 마법 충돌 사운드 재생
        SoundManager.Instance?.PlayWizardSpellSFX(spellInfo.spellName, SFX_Type.Hit, transform);

        if(TryGetComponent<Collision>(out Collision collision)){
            // 적중 효과 VFX
            HitVFX(GetHitVFXPrefab(), collision);
        }

        Destroy(gameObject);
    }
}

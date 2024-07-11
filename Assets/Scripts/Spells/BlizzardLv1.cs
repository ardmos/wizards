using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Netcode;
using UnityEngine;

public class BlizzardLv1 : AoESpell
{
    private const float DefaultSlowValue = 2f;
    private const float DefaultIntervalTime = 2f;

    [Header("블리자드 업그레이드 효과 적용을 위한 변수들")]
    public float slowValue;
    public ParticleSystem vfx_Blizzard_Lv1;
    public ParticleSystem msmoke;
    public ParticleSystem mflash;
    public ParticleSystem msparks;
    public ParticleSystem mglow;
    public ParticleSystem msnowflakes;
    public ParticleSystem mcircle;
    public ParticleSystem mlight;
    public ParticleSystem mfreezeCircle;
    public ParticleSystem mmsides;
    public ParticleSystem mmlight;
    public ParticleSystem mmsparks;
    public ParticleSystem mmsnowflakes;

    public override void InitAoESpell(SpellInfo spellInfoFromServer)
    {
        base.InitAoESpell(spellInfoFromServer);

        업그레이드효과적용();

        /// 보여지는 ParticleSystem을 업그레이드 현황에 맞게 수정하는 부분
        블리자드이펙트지속시간설정(spellInfo.lifetime);
        // 서버인 경우 변경 내용을 클라이언트측에 동기화
        if (IsServer)
            블리자드이펙트지속시간설정ClientRPC(NetworkObject, spellInfo.lifetime);

        블리자드파편이펙트세기설정(spellInfo.damage);
        // 서버인 경우 변경 내용을 클라이언트측에 동기화
        if (IsServer)
            블리자드파편이펙트세기설정ClientRPC(NetworkObject, spellInfo.damage);
        ///

        StartCoroutine(DestroyAfterDelay(spellInfo.lifetime));
    }

    private void 업그레이드효과적용()
    {
        foreach (BlizzardUpgradeOption upgradeOption in System.Enum.GetValues(typeof(BlizzardUpgradeOption)))
        {
            if (spellInfo.upgradeOptions[(int)upgradeOption] != 0)
            {
                switch (upgradeOption)
                {
                    case BlizzardUpgradeOption.IncreaseDuration:
                        // 블리자드의 지속시간이 1초 증가합니다
                        spellInfo.lifetime = SpellSpecifications.Instance.GetSpellDefaultSpec(SkillName.BlizzardLv1).lifetime + spellInfo.upgradeOptions[(int)upgradeOption];
                        break;
                    case BlizzardUpgradeOption.IncreaseDamage:
                        // "블리자드의 초당 공격력이 1만큼 증가합니다"
                        spellInfo.damage = (byte)(SpellSpecifications.Instance.GetSpellDefaultSpec(SkillName.BlizzardLv1).damage + spellInfo.upgradeOptions[(int)upgradeOption]);
                        break;
                    case BlizzardUpgradeOption.IncreaseSlowSpeed:
                        // "블리자드의 감속효과가 20%만큼 증가합니다"
                        slowValue = DefaultSlowValue * (1f + 0.2f * spellInfo.upgradeOptions[(int)upgradeOption]);
                        //Debug.Log($"new slowValue:{slowValue}");
                        break;
                }
            }
        }
    }

    private void 블리자드이펙트지속시간설정(float lifetime)
    {
        ParticleSystem[] particleSystems = {
            vfx_Blizzard_Lv1, msmoke, mflash, msparks, mglow, msnowflakes
        };

        ParticleSystem[] particleSystemsWithStartLifetime = {
            mcircle, mlight, mfreezeCircle, mmsides, mmlight, mmsparks, mmsnowflakes
        };

        // 모든 ParticleSystem을 정지합니다.
        foreach (var ps in particleSystems)
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
        foreach (var ps in particleSystemsWithStartLifetime)
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        // duration과 startLifetime을 설정합니다.
        foreach (var ps in particleSystems)
        {
            SetParticleSystemDuration(ps, lifetime);
        }
        foreach (var ps in particleSystemsWithStartLifetime)
        {
            SetParticleSystemDuration(ps, lifetime, true);
        }

        // 모든 ParticleSystem을 다시 시작합니다.
        foreach (var ps in particleSystems)
        {
            ps.Play();
        }
        foreach (var ps in particleSystemsWithStartLifetime)
        {
            ps.Play();
        }
    }

    [ClientRpc]
    private void 블리자드이펙트지속시간설정ClientRPC(NetworkObjectReference targetObject, float lifetime)
    {
        if (!targetObject.TryGet(out NetworkObject netObj))
        {
            return; // NetworkObject를 가져오는 데 실패하면 early return
        }

        if (netObj != NetworkObject)
        {
            return; // 현재 객체의 NetworkObject와 일치하지 않으면 early return
        }

        Debug.Log($"블리자드이펙트지속시간설정ClientRPC 호출! {spellInfo.ownerPlayerClientId}");
        블리자드이펙트지속시간설정(lifetime);        
    }

    private void 블리자드파편이펙트세기설정(float count)
    {
        // Emission 모듈을 가져옵니다.
        var emission = vfx_Blizzard_Lv1.emission;

        // Bursts 배열을 가져옵니다.
        ParticleSystem.Burst[] bursts = new ParticleSystem.Burst[emission.burstCount];
        emission.GetBursts(bursts);

        // Bursts 배열을 수정합니다.
        for (int i = 0; i < bursts.Length; i++)
        {
            bursts[i].count = new ParticleSystem.MinMaxCurve(count); // 원하는 count 값으로 변경
        }

        // Bursts 배열을 다시 설정합니다.
        emission.SetBursts(bursts);
    }

    [ClientRpc]
    private void 블리자드파편이펙트세기설정ClientRPC(NetworkObjectReference targetObject, float count)
    {
        if (!targetObject.TryGet(out NetworkObject netObj))
        {
            return; // NetworkObject를 가져오는 데 실패하면 early return
        }

        if (netObj != NetworkObject)
        {
            return; // 현재 객체의 NetworkObject와 일치하지 않으면 early return
        }

        Debug.Log($"블리자드파편이펙트세기설정ClientRPC 호출! {spellInfo.ownerPlayerClientId}");
        블리자드파편이펙트세기설정(count);
    }

    private void SetParticleSystemDuration(ParticleSystem ps, float duration, bool setStartLifetime = false)
    {
        var mainModule = ps.main;
        mainModule.duration = duration;
        if (setStartLifetime)
        {
            mainModule.startLifetime = duration;
        }
    }

    // DefaultIntervalTime 주기로 실행될 메서드
    private void DealDamage()
    {
        // 각 플레이어에게 대미지 주기. 공격자는 플레이어 자신으로 처리
        foreach (var player in playersInArea)
        {
            if (player == null || player.gameObject == null) continue;

            if (player.TryGetComponent<PlayerHPManagerServer>(out PlayerHPManagerServer playerHPManagerServer))
            {
                playerHPManagerServer.TakingDamage((sbyte)spellInfo.damage, player.GetComponent<PlayerClient>().OwnerClientId);
            }
            if (player.TryGetComponent<WizardRukeAIHPManagerServer>(out WizardRukeAIHPManagerServer wizardRukeAIHPManagerServer))
            {
                wizardRukeAIHPManagerServer.TakingDamage((sbyte)spellInfo.damage, player.GetComponent<WizardRukeAIServer>().AIClientId);
            }
            if (player.TryGetComponent<ChickenAIHPManagerServer>(out ChickenAIHPManagerServer chickenAIHPManagerServer))
            {
                chickenAIHPManagerServer.TakingDamage((sbyte)spellInfo.damage);
            }
        }
    }

    protected override void 플레이어전용트리거엔터(GameObject gameObject)
    {
        플레이어블리자드슬로우효과적용(gameObject);

        // 충돌한 플레이어를 리스트에 추가
        playersInArea.Add(gameObject);

        // 최초로 충돌한 플레이어인 경우에만 InvokeRepeating 메서드 호출
        if (playersInArea.Count == 1)
        {
            InvokeRepeating(nameof(DealDamage), 0f, DefaultIntervalTime);
        }
    }
    protected override void AI전용트리거엔터(GameObject gameObject)
    {
        AI블리자드슬로우효과적용(gameObject);

        // 충돌한 플레이어를 리스트에 추가
        playersInArea.Add(gameObject);

        // 최초로 충돌한 플레이어인 경우에만 InvokeRepeating 메서드 호출
        if (playersInArea.Count == 1)
        {
            InvokeRepeating(nameof(DealDamage), 0f, DefaultIntervalTime);
        }
    }

    protected override void 플레이어전용트리거엑싯(GameObject gameObject)
    {
        플레이어블리자드슬로우효과복구(gameObject);

        // 충돌을 끝낸 플레이어를 리스트에서 제거
        playersInArea.Remove(gameObject);

        // 더 이상 플레이어가 충돌 중이지 않다면 InvokeRepeating 메서드 중지
        if (playersInArea.Count == 0)
        {
            CancelInvoke(nameof(DealDamage));
        }
    }
    protected override void AI전용트리거엑싯(GameObject gameObject)
    {
        AI블리자드슬로우효과복구(gameObject);

        // 충돌을 끝낸 플레이어를 리스트에서 제거
        playersInArea.Remove(gameObject);

        // 더 이상 플레이어가 충돌 중이지 않다면 InvokeRepeating 메서드 중지
        if (playersInArea.Count == 0)
        {
            CancelInvoke(nameof(DealDamage));
        }
    }

    private void 플레이어블리자드슬로우효과적용(GameObject other)
    {
        if (!other) return;

        // 충돌한 플레이어 이동속도 저하.
        if (other.TryGetComponent<PlayerMovementServer>(out PlayerMovementServer playerMovement))
        {
            playerMovement.ReduceMoveSpeed(slowValue);
            //Debug.Log($"player{other.GetComponent<NetworkObject>().OwnerClientId} ReduceMoveSpeed result : {playerMovement.GetMoveSpeed()} ");
        }
        // 프로즌 이펙트 실행
        if (other.TryGetComponent<PlayerClient>(out PlayerClient playerClient))
        {
            //Debug.Log($"is playerClient found: {playerClient}");
            playerClient.ActivateFrozenEffectClientRPC();
        }
    }
    private void 플레이어블리자드슬로우효과복구(GameObject other)
    {
        if (!other) return;

        if (other.TryGetComponent<PlayerMovementServer>(out PlayerMovementServer playerMovement))
        {
            playerMovement.AddMoveSpeed(slowValue);
            //Debug.Log($"player{player.GetComponent<NetworkObject>().OwnerClientId} AddMoveSpeed result : {playerMovement.GetMoveSpeed()} ");
        }
        // 프로즌 이펙트 해제
        if (other.TryGetComponent<PlayerClient>(out PlayerClient playerClient))
        {
            //Debug.Log($"is playerClient found: {playerClient}");
            playerClient.DeactivateFrozenEffectClientRPC();
        }
    }

    private void AI블리자드슬로우효과적용(GameObject other)
    {
        if (!other) return;

        // 충돌한 플레이어 이동속도 저하.
        if (other.TryGetComponent<WizardRukeAIMovementServer>(out WizardRukeAIMovementServer aiPlayerMovement))
        {
            aiPlayerMovement.ReduceMoveSpeed(slowValue);
            //Debug.Log($"player{other.GetComponent<NetworkObject>().OwnerClientId} ReduceMoveSpeed result : {playerMovement.GetMoveSpeed()} ");
        }
        // 프로즌 이펙트 실행
        if (other.TryGetComponent<WizardRukeAIClient>(out WizardRukeAIClient aiPlayerClient))
        {
            //Debug.Log($"is playerClient found: {playerClient}");
            aiPlayerClient.ActivateFrozenEffectClientRPC();
        }
    }
    private void AI블리자드슬로우효과복구(GameObject other)
    {
        if (!other) return;

        if (other.TryGetComponent<WizardRukeAIMovementServer>(out WizardRukeAIMovementServer aiPlayerMovement))
        {
            aiPlayerMovement.AddMoveSpeed(slowValue);
            //Debug.Log($"player{other.GetComponent<NetworkObject>().OwnerClientId} AddMoveSpeed result : {playerMovement.GetMoveSpeed()} ");
        }
        // 프로즌 이펙트 해제
        if (other.TryGetComponent<WizardRukeAIClient>(out WizardRukeAIClient aiPlayerClient))
        {
            //Debug.Log($"is playerClient found: {playerClient}");
            aiPlayerClient.DeactivateFrozenEffectClientRPC();
        }
    }

    private IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        Destroy(gameObject);
    }

    public override void OnDestroy()
    {
        // 스킬이 사라질 때 아직 스킬의 영역에 남아있는 플레이어가 있는 경우 이동속도 복구시켜주기

        // 이동속도 복구
        foreach (var player in playersInArea)
        {
            if (player.CompareTag("Player"))
            {
                // 플레이어의 경우
                플레이어블리자드슬로우효과복구(player);
            }
            else if (player.CompareTag("AI"))
            {
                // AI의 경우
                AI블리자드슬로우효과복구(player);
            }
        }

        // 리스트 비우기
        playersInArea.Clear();
        // Invoke 중지
        CancelInvoke(nameof(DealDamage));
    }
}

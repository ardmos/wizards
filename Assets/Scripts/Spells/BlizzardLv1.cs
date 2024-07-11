using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Netcode;
using UnityEngine;

public class BlizzardLv1 : AoESpell
{
    private const float DefaultSlowValue = 2f;
    private const float DefaultIntervalTime = 2f;

    [Header("���ڵ� ���׷��̵� ȿ�� ������ ���� ������")]
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

        ���׷��̵�ȿ������();

        /// �������� ParticleSystem�� ���׷��̵� ��Ȳ�� �°� �����ϴ� �κ�
        ���ڵ�����Ʈ���ӽð�����(spellInfo.lifetime);
        // ������ ��� ���� ������ Ŭ���̾�Ʈ���� ����ȭ
        if (IsServer)
            ���ڵ�����Ʈ���ӽð�����ClientRPC(NetworkObject, spellInfo.lifetime);

        ���ڵ���������Ʈ���⼳��(spellInfo.damage);
        // ������ ��� ���� ������ Ŭ���̾�Ʈ���� ����ȭ
        if (IsServer)
            ���ڵ���������Ʈ���⼳��ClientRPC(NetworkObject, spellInfo.damage);
        ///

        StartCoroutine(DestroyAfterDelay(spellInfo.lifetime));
    }

    private void ���׷��̵�ȿ������()
    {
        foreach (BlizzardUpgradeOption upgradeOption in System.Enum.GetValues(typeof(BlizzardUpgradeOption)))
        {
            if (spellInfo.upgradeOptions[(int)upgradeOption] != 0)
            {
                switch (upgradeOption)
                {
                    case BlizzardUpgradeOption.IncreaseDuration:
                        // ���ڵ��� ���ӽð��� 1�� �����մϴ�
                        spellInfo.lifetime = SpellSpecifications.Instance.GetSpellDefaultSpec(SkillName.BlizzardLv1).lifetime + spellInfo.upgradeOptions[(int)upgradeOption];
                        break;
                    case BlizzardUpgradeOption.IncreaseDamage:
                        // "���ڵ��� �ʴ� ���ݷ��� 1��ŭ �����մϴ�"
                        spellInfo.damage = (byte)(SpellSpecifications.Instance.GetSpellDefaultSpec(SkillName.BlizzardLv1).damage + spellInfo.upgradeOptions[(int)upgradeOption]);
                        break;
                    case BlizzardUpgradeOption.IncreaseSlowSpeed:
                        // "���ڵ��� ����ȿ���� 20%��ŭ �����մϴ�"
                        slowValue = DefaultSlowValue * (1f + 0.2f * spellInfo.upgradeOptions[(int)upgradeOption]);
                        //Debug.Log($"new slowValue:{slowValue}");
                        break;
                }
            }
        }
    }

    private void ���ڵ�����Ʈ���ӽð�����(float lifetime)
    {
        ParticleSystem[] particleSystems = {
            vfx_Blizzard_Lv1, msmoke, mflash, msparks, mglow, msnowflakes
        };

        ParticleSystem[] particleSystemsWithStartLifetime = {
            mcircle, mlight, mfreezeCircle, mmsides, mmlight, mmsparks, mmsnowflakes
        };

        // ��� ParticleSystem�� �����մϴ�.
        foreach (var ps in particleSystems)
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
        foreach (var ps in particleSystemsWithStartLifetime)
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        // duration�� startLifetime�� �����մϴ�.
        foreach (var ps in particleSystems)
        {
            SetParticleSystemDuration(ps, lifetime);
        }
        foreach (var ps in particleSystemsWithStartLifetime)
        {
            SetParticleSystemDuration(ps, lifetime, true);
        }

        // ��� ParticleSystem�� �ٽ� �����մϴ�.
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
    private void ���ڵ�����Ʈ���ӽð�����ClientRPC(NetworkObjectReference targetObject, float lifetime)
    {
        if (!targetObject.TryGet(out NetworkObject netObj))
        {
            return; // NetworkObject�� �������� �� �����ϸ� early return
        }

        if (netObj != NetworkObject)
        {
            return; // ���� ��ü�� NetworkObject�� ��ġ���� ������ early return
        }

        Debug.Log($"���ڵ�����Ʈ���ӽð�����ClientRPC ȣ��! {spellInfo.ownerPlayerClientId}");
        ���ڵ�����Ʈ���ӽð�����(lifetime);        
    }

    private void ���ڵ���������Ʈ���⼳��(float count)
    {
        // Emission ����� �����ɴϴ�.
        var emission = vfx_Blizzard_Lv1.emission;

        // Bursts �迭�� �����ɴϴ�.
        ParticleSystem.Burst[] bursts = new ParticleSystem.Burst[emission.burstCount];
        emission.GetBursts(bursts);

        // Bursts �迭�� �����մϴ�.
        for (int i = 0; i < bursts.Length; i++)
        {
            bursts[i].count = new ParticleSystem.MinMaxCurve(count); // ���ϴ� count ������ ����
        }

        // Bursts �迭�� �ٽ� �����մϴ�.
        emission.SetBursts(bursts);
    }

    [ClientRpc]
    private void ���ڵ���������Ʈ���⼳��ClientRPC(NetworkObjectReference targetObject, float count)
    {
        if (!targetObject.TryGet(out NetworkObject netObj))
        {
            return; // NetworkObject�� �������� �� �����ϸ� early return
        }

        if (netObj != NetworkObject)
        {
            return; // ���� ��ü�� NetworkObject�� ��ġ���� ������ early return
        }

        Debug.Log($"���ڵ���������Ʈ���⼳��ClientRPC ȣ��! {spellInfo.ownerPlayerClientId}");
        ���ڵ���������Ʈ���⼳��(count);
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

    // DefaultIntervalTime �ֱ�� ����� �޼���
    private void DealDamage()
    {
        // �� �÷��̾�� ����� �ֱ�. �����ڴ� �÷��̾� �ڽ����� ó��
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

    protected override void �÷��̾�����Ʈ���ſ���(GameObject gameObject)
    {
        �÷��̾���ڵ彽�ο�ȿ������(gameObject);

        // �浹�� �÷��̾ ����Ʈ�� �߰�
        playersInArea.Add(gameObject);

        // ���ʷ� �浹�� �÷��̾��� ��쿡�� InvokeRepeating �޼��� ȣ��
        if (playersInArea.Count == 1)
        {
            InvokeRepeating(nameof(DealDamage), 0f, DefaultIntervalTime);
        }
    }
    protected override void AI����Ʈ���ſ���(GameObject gameObject)
    {
        AI���ڵ彽�ο�ȿ������(gameObject);

        // �浹�� �÷��̾ ����Ʈ�� �߰�
        playersInArea.Add(gameObject);

        // ���ʷ� �浹�� �÷��̾��� ��쿡�� InvokeRepeating �޼��� ȣ��
        if (playersInArea.Count == 1)
        {
            InvokeRepeating(nameof(DealDamage), 0f, DefaultIntervalTime);
        }
    }

    protected override void �÷��̾�����Ʈ���ſ���(GameObject gameObject)
    {
        �÷��̾���ڵ彽�ο�ȿ������(gameObject);

        // �浹�� ���� �÷��̾ ����Ʈ���� ����
        playersInArea.Remove(gameObject);

        // �� �̻� �÷��̾ �浹 ������ �ʴٸ� InvokeRepeating �޼��� ����
        if (playersInArea.Count == 0)
        {
            CancelInvoke(nameof(DealDamage));
        }
    }
    protected override void AI����Ʈ���ſ���(GameObject gameObject)
    {
        AI���ڵ彽�ο�ȿ������(gameObject);

        // �浹�� ���� �÷��̾ ����Ʈ���� ����
        playersInArea.Remove(gameObject);

        // �� �̻� �÷��̾ �浹 ������ �ʴٸ� InvokeRepeating �޼��� ����
        if (playersInArea.Count == 0)
        {
            CancelInvoke(nameof(DealDamage));
        }
    }

    private void �÷��̾���ڵ彽�ο�ȿ������(GameObject other)
    {
        if (!other) return;

        // �浹�� �÷��̾� �̵��ӵ� ����.
        if (other.TryGetComponent<PlayerMovementServer>(out PlayerMovementServer playerMovement))
        {
            playerMovement.ReduceMoveSpeed(slowValue);
            //Debug.Log($"player{other.GetComponent<NetworkObject>().OwnerClientId} ReduceMoveSpeed result : {playerMovement.GetMoveSpeed()} ");
        }
        // ������ ����Ʈ ����
        if (other.TryGetComponent<PlayerClient>(out PlayerClient playerClient))
        {
            //Debug.Log($"is playerClient found: {playerClient}");
            playerClient.ActivateFrozenEffectClientRPC();
        }
    }
    private void �÷��̾���ڵ彽�ο�ȿ������(GameObject other)
    {
        if (!other) return;

        if (other.TryGetComponent<PlayerMovementServer>(out PlayerMovementServer playerMovement))
        {
            playerMovement.AddMoveSpeed(slowValue);
            //Debug.Log($"player{player.GetComponent<NetworkObject>().OwnerClientId} AddMoveSpeed result : {playerMovement.GetMoveSpeed()} ");
        }
        // ������ ����Ʈ ����
        if (other.TryGetComponent<PlayerClient>(out PlayerClient playerClient))
        {
            //Debug.Log($"is playerClient found: {playerClient}");
            playerClient.DeactivateFrozenEffectClientRPC();
        }
    }

    private void AI���ڵ彽�ο�ȿ������(GameObject other)
    {
        if (!other) return;

        // �浹�� �÷��̾� �̵��ӵ� ����.
        if (other.TryGetComponent<WizardRukeAIMovementServer>(out WizardRukeAIMovementServer aiPlayerMovement))
        {
            aiPlayerMovement.ReduceMoveSpeed(slowValue);
            //Debug.Log($"player{other.GetComponent<NetworkObject>().OwnerClientId} ReduceMoveSpeed result : {playerMovement.GetMoveSpeed()} ");
        }
        // ������ ����Ʈ ����
        if (other.TryGetComponent<WizardRukeAIClient>(out WizardRukeAIClient aiPlayerClient))
        {
            //Debug.Log($"is playerClient found: {playerClient}");
            aiPlayerClient.ActivateFrozenEffectClientRPC();
        }
    }
    private void AI���ڵ彽�ο�ȿ������(GameObject other)
    {
        if (!other) return;

        if (other.TryGetComponent<WizardRukeAIMovementServer>(out WizardRukeAIMovementServer aiPlayerMovement))
        {
            aiPlayerMovement.AddMoveSpeed(slowValue);
            //Debug.Log($"player{other.GetComponent<NetworkObject>().OwnerClientId} AddMoveSpeed result : {playerMovement.GetMoveSpeed()} ");
        }
        // ������ ����Ʈ ����
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
        // ��ų�� ����� �� ���� ��ų�� ������ �����ִ� �÷��̾ �ִ� ��� �̵��ӵ� ���������ֱ�

        // �̵��ӵ� ����
        foreach (var player in playersInArea)
        {
            if (player.CompareTag("Player"))
            {
                // �÷��̾��� ���
                �÷��̾���ڵ彽�ο�ȿ������(player);
            }
            else if (player.CompareTag("AI"))
            {
                // AI�� ���
                AI���ڵ彽�ο�ȿ������(player);
            }
        }

        // ����Ʈ ����
        playersInArea.Clear();
        // Invoke ����
        CancelInvoke(nameof(DealDamage));
    }
}

using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Matchmaker.Models;
using UnityEngine;
/// <summary>
/// 
/// 1���� ���̾ ��ũ��Ʈ�Դϴ�.
/// 
/// !!! ���� ���
/// 1. �� �ɷ�ġ ����
/// </summary>
public class FireBallLv1 : FireSpell
{
    [Header("���̾�� ������")]
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
        // ���� ����ϴ� ���̾ VFX�� �ڿ������� �ϱ����� �κ�
        trails[0].SetActive(false);
    }

    public override void Shoot(Vector3 force, ForceMode forceMode)
    {
        base.Shoot(force, forceMode);

        // �������� ȿ�� Ȱ��ȭ
        trails[0].SetActive(true);
    }

    public override void InitSpellInfoDetail(SpellInfo spellInfoFromServer, GameObject spellOwnerObject)
    {
        base.InitSpellInfoDetail(spellInfoFromServer, spellOwnerObject);

        explosionRadius = 3f;
        piercingStack = 0f;
        damagePerSecond = 0;
        duration = 2f;
        // ���׷��̵� ��Ȳ ����
        ���׷��̵���Ȳ����();       
    }

    /// <summary>
    /// ���׷��̵� ��Ȳ�� �����մϴ�
    /// </summary>
    private void ���׷��̵���Ȳ����()
    {
        foreach (FireballUpgradeOption upgradeOption in System.Enum.GetValues(typeof(FireballUpgradeOption)))
        {
            if (spellInfo.upgradeOptions[(int)upgradeOption] != 0)
            {
                switch (upgradeOption)
                {
                    case FireballUpgradeOption.IncreaseSize:
                        // "���̾�� ũ�Ⱑ 10% �����մϴ�."
                        beam.localScale *= 1f + spellInfo.upgradeOptions[(int)upgradeOption]*0.1f;
                        ball.localScale *= 1f + spellInfo.upgradeOptions[(int)upgradeOption] * 0.1f;
                        smoke.localScale *= 1f + spellInfo.upgradeOptions[(int)upgradeOption] * 0.1f;
                        sphereCollider.radius *= 1f + spellInfo.upgradeOptions[(int)upgradeOption] * 0.1f;
                        break;
                    case FireballUpgradeOption.AddDotDamage:
                        // "���̾�� ���� ���� 5�� ���� �ʴ� 1�� ȭ�� ���ظ� �Խ��ϴ�."
                        damagePerSecond += (sbyte)spellInfo.upgradeOptions[(int)upgradeOption];
                        break;
                    case FireballUpgradeOption.IncreaseExplosionRadius:
                        // "���̾�� ���� ������ 1 ������ŵ�ϴ�."
                        explosionRadius += spellInfo.upgradeOptions[(int)upgradeOption];
                        break;
                    case FireballUpgradeOption.ReduceCooldown:
                        // "���̾�� ���� ��� �ð��� 20% �����մϴ�."
                        float defaultCooltime = SpellSpecifications.Instance.GetSpellDefaultSpec(SkillName.FireBallLv1).coolTime;
                        float reductionPercentage = 0.2f * (float)spellInfo.upgradeOptions[(int)upgradeOption];
                        spellInfo.coolTime = defaultCooltime * (1 - reductionPercentage);
                        //Debug.Log($"{SpellSpecifications.Instance.GetSpellDefaultSpec(SkillName.FireBallLv1).coolTime} - 0.2f * {spellInfo.upgradeOptions[(int)upgradeOption]}. ���̾ ��Ÿ��:{spellInfo.coolTime}");
                        break;
                    case FireballUpgradeOption.AddPiercing:
                        // "���̾�� ���� �����մϴ�."
                        piercingStack += spellInfo.upgradeOptions[(int)upgradeOption];
                        break;
                }

            }
        }
    }

    /// <summary>
    /// 2. CollisionEnter �浹 ó�� (���� ���� ���)
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter(Collision collision)
    {
        // ���������� ó��.
        if (IsClient) return;

        //Debug.Log($"���� �浹! ����:{gameObject.name}, �浹ü:{collision.gameObject.name}");

        // �浹�� �ߺ� ó���ϴ°��� �����ϱ� ���� ó��
        GetComponent<Collider>().enabled = false;

        // �������� ����� ���� ó��
        ������ó��(collision);

        // Ȥ�� �����ڰ� Casting ���¿����� Cooltime ���·� �Ѱ��ֱ� 
        ����ų��ĳ���û��¿������();

        // ���� �浹 ���� ���
        SoundManager.Instance?.PlayWizardSpellSFX(spellInfo.spellName, SFX_Type.Hit, transform);

        // ���� ȿ�� VFX
        HitVFX(GetHitVFXPrefab(), collision);

        if (!�Ǿ��ȿ������()) return;

        ���ݸ������浹�Ѱ��(collision);

        Destroy(gameObject, 0.2f);
    }

    private void ������ó��(Collision collision)
    {
        ulong spellOwnerClientId = GetSpellInfo().ownerPlayerClientId;
        // �浹 ����
        Vector3 explosionPosition = collision.contacts[0].point;
        Collider[] colliders = Physics.OverlapSphere(explosionPosition, explosionRadius);
        // ���� �� �浹ü �ν�
        foreach (Collider hit in colliders)
        {
            // �����ڴ� ���� �ȹ޵��� ����
            if (hit.gameObject == spellOwnerObject) continue;

            // �浹�Ѱ� �÷��̾��� ���, �÷��̾��� �ǰ� ����� �ش� �÷��̾��� SpellManager �˸��ϴ�. 
            if (hit.CompareTag("Player"))
            {
                if (GetSpellInfo() == null) return;

                if (hit.TryGetComponent<PlayerServer>(out PlayerServer playerServer))
                {
                    sbyte damage = (sbyte)GetSpellInfo().damage;
                    // �÷��̾� �ǰ��� �������� ó��
                    playerServer.PlayerGotHitOnServer(damage, spellOwnerClientId);

                    // ��Ʈ ������ ����
                    if (damagePerSecond > 0)
                        playerServer.StartCoroutine(playerServer.TakeDamageOverTime(damagePerSecond, duration, spellOwnerClientId));
                }
            }
            // AI�÷��̾��� ��� ó��
            else if (hit.CompareTag("AI"))
            {
                if (GetSpellInfo() == null) return;

                // WizardRukeAI Ȯ��.  ���� �ٸ� AI�߰� �� ����.         
                if (hit.TryGetComponent<WizardRukeAIServer>(out WizardRukeAIServer aiPlayer))
                {
                    sbyte damage = (sbyte)GetSpellInfo().damage;
                    // �÷��̾� �ǰ��� �������� ó��
                    aiPlayer.PlayerGotHitOnServer(damage, spellOwnerClientId, spellOwnerObject);

                    // ��Ʈ ������ ����
                    if (damagePerSecond > 0)
                        aiPlayer.StartCoroutine(aiPlayer.TakeDamageOverTime(damagePerSecond, duration, spellOwnerClientId));
                }
            }
            // ��Ÿ ������Ʈ �浹
            else
            {
                //Debug.Log($"{collider.name} Hit!");
            }
        }
    }

    private void ����ų��ĳ���û��¿������()
    {
        // �����ڰ� AI�� ���
        if (spellOwnerObject.TryGetComponent<WizardRukeAISpellManagerServer>(out WizardRukeAISpellManagerServer wizardRukeAISpellManagerServer))
        {
            if (wizardRukeAISpellManagerServer.GetSpellInfo(GetSpellInfo().spellName).spellState == SpellState.Aiming)
            {
                //Debug.Log($"ĳ���� ���¿��� �����߽��ϴ�!!! ��ų ���¸� �����մϴ�. spellState:{spellManagerServerWizard.GetSpellInfo(GetSpellInfo().spellName).spellState} -> ");
                int spellIndex = wizardRukeAISpellManagerServer.GetSpellIndexBySpellName(GetSpellInfo().spellName);
                if (spellIndex != -1)
                {
                    wizardRukeAISpellManagerServer.UpdatePlayerSpellState((ushort)spellIndex, SpellState.Cooltime);
                    wizardRukeAISpellManagerServer.playerAnimator.UpdateWizardMaleAnimationOnServer(WizardMaleAnimState.ShootingMagic);
                    //Debug.Log($"{spellManagerServerWizard.GetSpellInfo(GetSpellInfo().spellName).spellState}.");
                }
                else Debug.Log($"���� �ε����� ã�� ���߽��ϴ�.");
            }
        }
        // �����ڰ� �÷��̾��� ���
        else if (spellOwnerObject.TryGetComponent<SpellManagerServerWizard>(out SpellManagerServerWizard spellManagerServerWizard))
        {
            if (spellManagerServerWizard.GetSpellInfo(GetSpellInfo().spellName).spellState == SpellState.Aiming)
            {
                //Debug.Log($"ĳ���� ���¿��� �����߽��ϴ�!!! ��ų ���¸� �����մϴ�. spellState:{spellManagerServerWizard.GetSpellInfo(GetSpellInfo().spellName).spellState} -> ");
                int spellIndex = spellManagerServerWizard.GetSpellIndexBySpellName(GetSpellInfo().spellName);
                if (spellIndex != -1)
                {
                    spellManagerServerWizard.UpdatePlayerSpellState((ushort)spellIndex, SpellState.Cooltime);
                    spellManagerServerWizard.playerAnimator.UpdateWizardMaleAnimationOnServer(WizardMaleAnimState.ShootingMagic);
                    //Debug.Log($"{spellManagerServerWizard.GetSpellInfo(GetSpellInfo().spellName).spellState}.");
                }
                else Debug.Log($"���� �ε����� ã�� ���߽��ϴ�.");
            }
        }

    }

    private bool �Ǿ��ȿ������()
    {
        // �Ǿ�� ������ ���������� �����ı� ���� ���� ���, �Ǿ�� ������ ���ҽ�ŵ�ϴ�
        if (--piercingStack >= 0)
        {
            Debug.Log($"�Ǿ�� ���� ����!{piercingStack}");
            return true;
        }
        return false;
    }

    private void ���ݸ������浹�Ѱ��(Collision collision)
    {
        // �浹�Ѱ� ���ݸ����̸鼭 ���̾ ��ü�� �浹�� ���, � ������ ��Ƴ����� ��꿡 ��
        if (collision.collider.CompareTag("AttackSpell"))
        {
            SpellHitHandlerOnServer(collision.collider);
        }
        else if (collision.collider.CompareTag("AttackSkill"))
        {

        }
    }
}

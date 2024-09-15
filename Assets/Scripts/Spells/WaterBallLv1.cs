using System.Collections;
using Unity.Netcode;
using Unity.Services.Matchmaker.Models;
using UnityEngine;
/// <summary>
/// 1���� ���ͺ� ��ũ��Ʈ�Դϴ�.
/// </summary>
public class WaterBallLv1 : WaterSpell
{
    [Header("���ͺ��� ������")]
    public HomingMissile homingMissile;
    public bool isSplashON;
    public float explosionRadius = 3f;

    float defaultLifetime;
    float increaseLifetime;

    public override void InitSpellInfoDetail(SpellInfo spellInfoFromServer, GameObject spellOwnerObject)
    {
        base.InitSpellInfoDetail(spellInfoFromServer, spellOwnerObject);

        isSplashON = false;
        defaultLifetime = spellInfo.lifetime;

        // ���׷��̵� ��Ȳ ����
        ���׷��̵���Ȳ����();

        StartCoroutine(DestroyAfterDelay(defaultLifetime + increaseLifetime));
    }

    /// <summary>
    /// ���׷��̵� ��Ȳ�� �����մϴ�
    /// </summary>
    private void ���׷��̵���Ȳ����()
    {
        foreach (WaterballUpgradeOption upgradeOption in System.Enum.GetValues(typeof(WaterballUpgradeOption)))
        {
            if (spellInfo.upgradeOptions[(int)upgradeOption] != 0)
            {
                switch (upgradeOption)
                {
                    case WaterballUpgradeOption.IncreaseSpeed:
                        // "���ͺ��� �ӵ��� 30% �����մϴ�."
                        spellInfo.moveSpeed = SpellSpecifications.Instance.GetSpellDefaultSpec(SpellName.WaterBallLv1).moveSpeed - 0.2f * (float)spellInfo.upgradeOptions[(int)upgradeOption];
                        break;
                    case WaterballUpgradeOption.IncreaseHomingRange:
                        // "���ͺ��� ���� Ÿ�� �ν� ������ 20% �����մϴ�."
                        float defaultRange = homingMissile.GetMaxHomingRange();
                        float increasePercentage = 0.2f * (float)spellInfo.upgradeOptions[(int)upgradeOption];
                        homingMissile.SetMaxHomingRange(defaultRange * (1 + increasePercentage));
                        //damagePerSecond += (sbyte)spellInfo.upgradeOptions[(int)upgradeOption];
                        break;
                    case WaterballUpgradeOption.AddSplashDamage:
                        // "���ͺ��� ���� �� �ֺ��� ���� ���ظ� �����ϴ�."
                        isSplashON = true;
                        break;
                    case WaterballUpgradeOption.ReduceCooldown:
                        // "���ͺ��� ���� ��� �ð��� 20% �����մϴ�."
                        float defaultCooltime = SpellSpecifications.Instance.GetSpellDefaultSpec(SpellName.FireBallLv1).coolTime;
                        float reductionPercentage = 0.2f * (float)spellInfo.upgradeOptions[(int)upgradeOption];
                        spellInfo.coolTime = defaultCooltime * (1 - reductionPercentage);
                        //Debug.Log($"{SpellSpecifications.Instance.GetSpellDefaultSpec(SkillName.FireBallLv1).coolTime} - 0.2f * {spellInfo.upgradeOptions[(int)upgradeOption]}. ���̾ ��Ÿ��:{spellInfo.coolTime}");
                        break;
                    case WaterballUpgradeOption.IncreaseRange:
                        // "���ͺ��� ��Ÿ��� 50% �����մϴ�."
                        defaultLifetime = spellInfo.lifetime;
                        increaseLifetime = defaultLifetime * 0.5f * spellInfo.upgradeOptions[(int)upgradeOption];
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

        if (isSplashON)
        {
            ������ó��(collision);
        }
        else
        {
            ���ϵ�ó��(collision);
        }

        // Ȥ�� �����ڰ� Casting ���¿����� Cooltime ���·� �Ѱ��ֱ� 
        ����ų��ĳ���û��¿������();

        // ���� �浹 ���� ���
        SoundManager.Instance?.PlayWizardSpellSFX(spellInfo.spellName, SFX_Type.Hit, transform);

        // ���� ȿ�� VFX
        HitVFX(GetHitVFXPrefab(), collision);

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

            if (GetSpellInfo() == null) return;

            // �浹�Ѱ� �÷��̾��� ���, �÷��̾��� �ǰ� ����� �ش� �÷��̾��� SpellManager �˸��ϴ�. 
            if (hit.CompareTag("Player"))
            {
                if (hit.TryGetComponent<PlayerHPManagerServer>(out PlayerHPManagerServer playerServer))
                {
                    sbyte damage = (sbyte)GetSpellInfo().damage;
                    // �÷��̾� �ǰ��� �������� ó��
                    playerServer.TakingDamageWithCameraShake(damage, spellOwnerClientId, spellOwnerObject);
                }
            }
            // AI�÷��̾��� ��� ó��
            else if (hit.CompareTag("AI"))
            {
                // WizardRukeAI Ȯ��.  ���� �ٸ� AI�߰� �� ����.         
                if (hit.TryGetComponent<WizardRukeAIHPManagerServer>(out WizardRukeAIHPManagerServer aiPlayer))
                {
                    sbyte damage = (sbyte)GetSpellInfo().damage;
                    // �÷��̾� �ǰ��� �������� ó��
                    aiPlayer.TakingDamageWithCameraShake(damage, spellOwnerClientId, spellOwnerObject);
                }
            }
            // Monster�� ��� ó��
            else if (hit.CompareTag("Monster"))
            {
                // WizardRukeAI Ȯ��.  ���� �ٸ� AI�߰� �� ����.         
                if (hit.TryGetComponent<ChickenAIHPManagerServer>(out ChickenAIHPManagerServer chickenAIHPManagerServer))
                {
                    sbyte damage = (sbyte)GetSpellInfo().damage;
                    // �÷��̾� �ǰ��� �������� ó��
                    ///// �±� �߰�. ���� �޼��� �ۼ�. ī�޶���ŷ. chickenAIHPManagerServer.TakingDamage(damage);
                    chickenAIHPManagerServer.TakingDamageWithCameraShake(damage, spellOwnerObject);
                }
            }
            // ��Ÿ ������Ʈ �浹
            else
            {
                //Debug.Log($"{collider.name} Hit!");
            }
        }
    }

    private void ���ϵ�ó��(Collision collision)
    {
        Collider collider = collision.collider;
        ulong spellOwnerClientId = GetSpellInfo().ownerPlayerClientId;

        // �����ڴ� ���� �ȹ޵��� ����
        if (collision.gameObject == spellOwnerObject) return;
        if (GetSpellInfo() == null) return;

        // �浹�Ѱ� �÷��̾��� ���, �÷��̾��� �ǰ� ����� �ش� �÷��̾��� SpellManager �˸��ϴ�. 
        if (collider.CompareTag("Player"))
        {
            if(collider.TryGetComponent<PlayerHPManagerServer>(out PlayerHPManagerServer player))
            {
                sbyte damage = (sbyte)GetSpellInfo().damage;
                // �÷��̾� �ǰ��� �������� ó��
                player.TakingDamageWithCameraShake(damage, spellOwnerClientId, spellOwnerObject);
            }
        }
        // AI�÷��̾��� ��� ó��
        else if (collider.CompareTag("AI"))
        {
            // WizardRukeAI Ȯ��.  ���� �ٸ� AI�߰� �� ����.         
            if (collider.TryGetComponent<WizardRukeAIHPManagerServer>(out WizardRukeAIHPManagerServer aiPlayer))
            {
                sbyte damage = (sbyte)GetSpellInfo().damage;
                // �÷��̾� �ǰ��� �������� ó��
                aiPlayer.TakingDamageWithCameraShake(damage, spellOwnerClientId, spellOwnerObject);
            }
        }
        // Monster�� ��� ó��
        else if (collider.CompareTag("Monster"))
        {
            // WizardRukeAI Ȯ��.  ���� �ٸ� AI�߰� �� ����.         
            if (collider.TryGetComponent<ChickenAIHPManagerServer>(out ChickenAIHPManagerServer chickenAIHPManagerServer))
            {
                sbyte damage = (sbyte)GetSpellInfo().damage;
                // �÷��̾� �ǰ��� �������� ó��
                ///// �±� �߰�. ���� �޼��� �ۼ�. ī�޶���ŷ. chickenAIHPManagerServer.TakingDamage(damage);
                chickenAIHPManagerServer.TakingDamageWithCameraShake(damage, spellOwnerObject);
            }
        }
        // ��Ÿ ������Ʈ �浹
        else
        {
            //Debug.Log($"{collider.name} Hit!");
        }
    }

    private void ����ų��ĳ���û��¿������()
    {
        // �����ڰ� AI�� ���
        if (spellOwnerObject.TryGetComponent<WizardRukeAISpellManagerServer>(out WizardRukeAISpellManagerServer wizardRukeAISpellManagerServer))
        {
            if (wizardRukeAISpellManagerServer.GetSpellInfo(GetSpellInfo().spellName).spellState == SpellState.Casting)
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
            if (spellManagerServerWizard.GetSpellInfo(GetSpellInfo().spellName).spellState == SpellState.Casting)
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

    private void ���ݸ������浹�Ѱ��(Collision collision)
    {
        // �浹�Ѱ� ���ݸ����� ���, � ������ ��Ƴ����� ��꿡 ��
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

        // ���� �浹 ���� ���
        SoundManager.Instance?.PlayWizardSpellSFX(spellInfo.spellName, SFX_Type.Hit, transform);

        // ���� ȿ�� VFX
        HitVFX(GetHitVFXPrefab());

        Destroy(gameObject);
    }
}

using Unity.Netcode;
using Unity.Services.Matchmaker.Models;
using UnityEngine;
/// <summary>
/// 1���� ���ͺ� ��ũ��Ʈ�Դϴ�.
/// </summary>
public class WaterBallLv1 : WaterSpell
{
    [Header("���ͺ��� ������")]
    public 

    public override void OnNetworkSpawn()
    {
        // ���׷��̵� ��Ȳ Ȯ��
        if (spellInfo.upgradeOptions.Length != System.Enum.GetValues(typeof(WaterballUpgradeOption)).Length) return;
        foreach (WaterballUpgradeOption upgradeOption in System.Enum.GetValues(typeof(WaterballUpgradeOption)))
        {
            Debug.Log($"{upgradeOption} : {spellInfo.upgradeOptions[(int)upgradeOption]}");
        }
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
        foreach (WaterballUpgradeOption upgradeOption in System.Enum.GetValues(typeof(WaterballUpgradeOption)))
        {
            if (spellInfo.upgradeOptions[(int)upgradeOption] != 0)
            {
                switch (upgradeOption)
                {
                    case WaterballUpgradeOption.IncreaseSpeed:
                        // "���ͺ��� �ӵ��� 30% �����մϴ�."
                        spellInfo.moveSpeed = SpellSpecifications.Instance.GetSpellDefaultSpec(SkillName.WaterBallLv1).moveSpeed - 0.2f * (float)spellInfo.upgradeOptions[(int)upgradeOption];
                        break;
                    case WaterballUpgradeOption.IncreaseHomingRange:
                        // "���ͺ��� ���� Ÿ�� �ν� ������ 20% �����մϴ�."
                        damagePerSecond += (sbyte)spellInfo.upgradeOptions[(int)upgradeOption];
                        break;
                    case WaterballUpgradeOption.AddSplashDamage:
                        // "���ͺ��� ���� �� �ֺ��� ���� ���ظ� �����ϴ�."
                        explosionRadius += spellInfo.upgradeOptions[(int)upgradeOption];
                        break;
                    case WaterballUpgradeOption.ReduceCooldown:
                        // "���ͺ��� ���� ��� �ð��� 20% �����մϴ�."
                        float defaultCooltime = SpellSpecifications.Instance.GetSpellDefaultSpec(SkillName.FireBallLv1).coolTime;
                        float reductionPercentage = 0.2f * (float)spellInfo.upgradeOptions[(int)upgradeOption];
                        spellInfo.coolTime = defaultCooltime * (1 - reductionPercentage);
                        //Debug.Log($"{SpellSpecifications.Instance.GetSpellDefaultSpec(SkillName.FireBallLv1).coolTime} - 0.2f * {spellInfo.upgradeOptions[(int)upgradeOption]}. ���̾ ��Ÿ��:{spellInfo.coolTime}");
                        break;
                    case WaterballUpgradeOption.IncreaseRange:
                        // "���ͺ��� ��Ÿ��� 50% �����մϴ�."
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

        Collider collider = collision.collider;
        ulong spellOwnerClientId = GetSpellInfo().ownerPlayerClientId;

        // �浹�Ѱ� ���ݸ����� ���, � ������ ��Ƴ����� ��꿡 ��
        if (collider.CompareTag("AttackSpell"))
        {
            SpellHitHandlerOnServer(collider);
        }
        else if (collider.CompareTag("AttackSkill"))
        {

        }
        // �浹�Ѱ� �÷��̾��� ���, �÷��̾��� �ǰ� ����� �ش� �÷��̾��� SpellManager �˸��ϴ�. 
        else if (collider.CompareTag("Player"))
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
            // �÷��̾� �ǰ��� �������� ó��
            player.PlayerGotHitOnServer(damage, spellOwnerClientId);
        }
        // AI�÷��̾��� ��� ó��
        else if (collider.CompareTag("AI"))
        {
            if (GetSpellInfo() == null) return;

            // WizardRukeAI Ȯ��.  ���� �ٸ� AI�߰� �� ����.         
            if (collider.TryGetComponent<WizardRukeAIServer>(out WizardRukeAIServer aiPlayer))
            {
                sbyte damage = (sbyte)GetSpellInfo().level;
                // �÷��̾� �ǰ��� �������� ó��
                aiPlayer.PlayerGotHitOnServer(damage, spellOwnerClientId, spellOwnerObject);
            }
        }
        // ��Ÿ ������Ʈ �浹
        else
        {
            //Debug.Log($"{collider.name} Hit!");
        }

        // Ȥ�� �����ڰ� Casting ���¿����� Cooltime ���·� �Ѱ��ֱ� 
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

        // ���� �浹 ���� ���
        SoundManager.Instance?.PlayWizardSpellSFX(spellInfo.spellName, SFX_Type.Hit, transform);

        // ���� ȿ�� VFX
        HitVFX(GetHitVFXPrefab(), collision);

        Destroy(gameObject, 0.2f);
    }
}

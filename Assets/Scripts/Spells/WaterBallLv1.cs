using Unity.Netcode;
using UnityEngine;
/// <summary>
/// 1���� ���ͺ� ��ũ��Ʈ�Դϴ�.
/// </summary>
public class WaterBallLv1 : WaterSpell
{
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

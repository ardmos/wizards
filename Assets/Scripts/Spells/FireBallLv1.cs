using Unity.Netcode;
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
    // ���� ����ϴ� ���̾ VFX�� �ڿ������� �ϱ����� �κ�
    public override void OnNetworkSpawn()
    {
        trails[0].SetActive(false);
    }

    public override void Shoot(Vector3 force, ForceMode forceMode)
    {
        base.Shoot(force, forceMode);

        // �������� ȿ�� Ȱ��ȭ
        trails[0].SetActive(true);
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
        ulong spellOwnerClientId = GetSpellInfo().ownerPlayerClientId;

        // �������� ����� ���� ó��
        // �浹�� ��ü�� Player �Ǵ� AI �±׸� ������ �ִ��� Ȯ��
        if (collision.collider.CompareTag("Player") || collision.collider.CompareTag("AI"))
        {
            // �浹 ����
            Vector3 explosionPosition = collision.contacts[0].point;
            // ���� 3 ���� ��� ��ü ã��
            Collider[] colliders = Physics.OverlapSphere(explosionPosition, 3f);

            foreach (Collider hit in colliders)
            {
                // ������ ������

                // �浹�Ѱ� ���ݸ����̸鼭 ���̾ ��ü�� �浹�� ���, � ������ ��Ƴ����� ��꿡 ��
                if (hit.gameObject == collision.gameObject && hit.CompareTag("AttackSpell"))
                {
                    SpellHitHandlerOnServer(hit);
                }
                else if (hit.CompareTag("AttackSkill"))
                {

                }
                // �浹�Ѱ� �÷��̾��� ���, �÷��̾��� �ǰ� ����� �ش� �÷��̾��� SpellManager �˸��ϴ�. 
                else if (hit.CompareTag("Player"))
                {
                    // �����ڴ� ���� �ȹ޵��� ����
                    if (hit.gameObject.layer == shooterLayer) continue;

                    if (GetSpellInfo() == null) return;

                    if (hit.TryGetComponent<PlayerServer>(out PlayerServer playerServer))
                    {
                        sbyte damage = (sbyte)GetSpellInfo().level;
                        // �÷��̾� �ǰ��� �������� ó��
                        playerServer.PlayerGotHitOnServer(damage, spellOwnerClientId);
                    }
                }
                // AI�÷��̾��� ��� ó��
                else if (hit.CompareTag("AI"))
                {
                    // �����ڴ� ���� �ȹ޵��� ����
                    if (hit.gameObject.layer == shooterLayer) continue;

                    if (GetSpellInfo() == null) return;

                    // WizardRukeAI Ȯ��.  ���� �ٸ� AI�߰� �� ����.         
                    if (hit.TryGetComponent<WizardRukeAIServer>(out WizardRukeAIServer aiPlayer))
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
            }
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
        else if(spellOwnerObject.TryGetComponent<SpellManagerServerWizard>(out SpellManagerServerWizard spellManagerServerWizard))
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

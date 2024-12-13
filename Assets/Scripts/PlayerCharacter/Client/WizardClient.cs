using System;
using Unity.Netcode;

/// <summary>
/// Ŭ���̾�Ʈ ������ Wizard ĳ������ Ư���� �ൿ�� �����ϴ� Ŭ�����Դϴ�.
/// PlayerClient�� ��ӹް� ICharacter �������̽��� �����մϴ�.
/// </summary>
public class WizardClient : PlayerClient
{
    #region Fields and Properties
    /// <summary>
    /// Wizard ĳ������ ���� �Ŵ��� ������Ʈ�Դϴ�.
    /// </summary>
    public SpellManagerClientWizard spellManagerClientWizard;
    #endregion

    #region Initialization
    [ClientRpc]
    public void InitializeWizardClientRPC(SpellName[] spellNames)
    {
        InitializePlayerClient();
        // ���� ���� ������ GamePad UI�� �ݿ�          
        GameSceneUIManager.Instance.gamePadUIController.UpdateSpellUI(spellNames);
        // ���� ���� ������ �������� ��ų �Ŵ����� �ʱ�ȭ
        spellManagerClientWizard.InitPlayerSpellInfoListClient(spellNames);
    }
    #endregion

    #region Input Handling
    /// <summary>
    /// Attack1 ��ư�� ������ �� ȣ��Ǵ� �޼����Դϴ�. ��ų�� ĳ�����ϱ� �����մϴ�.
    /// </summary>
    protected override void OnAttack_1_Started()
    {
        spellManagerClientWizard.CastingNormalSpell(0);
    }
    /// <summary>
    /// Attack1 ��ư�� ������ �� ȣ��Ǵ� �޼����Դϴ�. ��ų�� �����մϴ�.
    /// </summary>
    protected override void OnAttack_1_Ended()
    {
        spellManagerClientWizard.ReleaseNormalSpell(0);
    }

    /// <summary>
    /// Attack2 ��ư�� ������ �� ȣ��Ǵ� �޼����Դϴ�. ��ų�� ĳ�����ϱ� �����մϴ�.
    /// </summary>
    protected override void OnAttack_2_Started()
    {
        spellManagerClientWizard.CastingNormalSpell(1);
    }
    /// <summary>
    /// Attack2 ��ư�� ������ �� ȣ��Ǵ� �޼����Դϴ�. ��ų�� �����մϴ�.
    /// </summary>
    protected override void OnAttack_2_Ended()
    {
        spellManagerClientWizard.ReleaseNormalSpell(1);
    }

    /// <summary>
    /// Attack3 ��ư�� ������ �� ȣ��Ǵ� �޼����Դϴ�. ��ų�� ĳ�����ϱ� �����մϴ�.
    /// </summary>
    protected override void OnAttack_3_Started()
    {
        spellManagerClientWizard.CastingBlizzard();
    }
    /// <summary>
    /// Attack3 ��ư�� ������ �� ȣ��Ǵ� �޼����Դϴ�. ��ų�� �����մϴ�.
    /// </summary>
    protected override void OnAttack_3_Ended()
    {
        spellManagerClientWizard.ReleaseBlizzard();
    }

    /// <summary>
    /// Defence ��ư�� ������ �� ȣ��Ǵ� �޼����Դϴ�.
    /// Wizard�� Defence��ų�� ��� �ߵ��Ǳ� ������ ���⼭ �ٷ� ������ŵ�ϴ�.
    /// </summary>
    protected override void GameInput_OnDefenceStarted(object sender, EventArgs e)
    {
        spellManagerClientWizard.ActivateShield();
    }
    /// <summary>
    /// Defence ��ư�� ������ �� ȣ��Ǵ� �޼����Դϴ�.
    /// Wizard�� Defence��ų�� ��� �ߵ��Ǳ� ������ ���ٸ� �۾��� ���� �ʽ��ϴ�.
    /// </summary>
    protected override void GameInput_OnDefenceEnded(object sender, EventArgs e) { }
    #endregion
}

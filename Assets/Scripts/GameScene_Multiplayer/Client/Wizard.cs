using System;
using Unity.Netcode;

/// <summary>
/// Wizard ĳ������ Ư���� �ൿ�� �����ϴ� Ŭ�����Դϴ�.
/// PlayerClient�� ��ӹް� ICharacter �������̽��� �����մϴ�.
/// </summary>
public class Wizard : PlayerClient, ICharacter
{
    #region Fields and Properties
    /// <summary>
    /// Wizard ĳ������ ���� �Ŵ��� ������Ʈ�Դϴ�.
    /// </summary>
    public SpellManagerClientWizard spellManagerClientWizard;

    /// <summary>
    /// ĳ���� Ŭ������ ��Ÿ���ϴ�.
    /// </summary>
    public Character characterClass { get; set; } = Character.Wizard;
    /// <summary>
    /// ���� ü���� ��Ÿ���ϴ�.
    /// </summary>
    public sbyte hp { get; set; } = 5;
    /// <summary>
    /// �ִ� ü���� ��Ÿ���ϴ�.
    /// </summary>
    public sbyte maxHp { get; set; } = 5;
    /// <summary>
    /// �̵� �ӵ��� ��Ÿ���ϴ�.
    /// </summary>
    public float moveSpeed { get; set; } = 4f;
    /// <summary>
    /// Wizard�� ����� �� �ִ� ��ų ����Դϴ�.
    /// </summary>
    public SpellName[] skills { get; set; } = new SpellName[]{
                SpellName.FireBallLv1,
                SpellName.WaterBallLv1,
                SpellName.BlizzardLv1,
                SpellName.MagicShieldLv1
                };
    #endregion

    #region Initialization
    /// <summary>
    /// �÷��̾ �ʱ�ȭ�ϴ� ClientRpc �޼����Դϴ�.
    /// </summary>
    [ClientRpc]
    public override void InitializePlayerClientRPC()
    {
        base.InitializePlayerClientRPC();
        // ���� skill ������ GamePad UI�� �ݿ�          
        GameSceneUIManager.Instance.gamePadUIController.UpdateSpellUI(skills);
        // ���� skill ������ �������� ��ų �Ŵ����� �ʱ�ȭ
        spellManagerClientWizard.InitPlayerSpellInfoListClient(skills);
    }
    #endregion

    #region Character Data
    /// <summary>
    /// ĳ���� �����͸� ��ȯ�ϴ� �޼����Դϴ�.
    /// </summary>
    /// <returns>ICharacter �������̽��� ������ ���� ��ü</returns>
    public ICharacter GetCharacterData()
    {
        return this;
    }
    #endregion

    #region Input Handling
    /// <summary>
    /// Attack1 ��ư�� ������ �� ȣ��Ǵ� �޼����Դϴ�. ��ų�� ĳ�����ϱ� �����մϴ�.
    /// </summary>
    protected override void GameInput_OnAttack1Started(object sender, EventArgs e)
    {
        spellManagerClientWizard.CastingNormalSpell(0);
    }
    /// <summary>
    /// Attack1 ��ư�� ������ �� ȣ��Ǵ� �޼����Դϴ�. ��ų�� �����մϴ�.
    /// </summary>
    protected override void GameInput_OnAttack1Ended(object sender, EventArgs e)
    {
        spellManagerClientWizard.ShootNormalSpell(0);
    }

    /// <summary>
    /// Attack2 ��ư�� ������ �� ȣ��Ǵ� �޼����Դϴ�. ��ų�� ĳ�����ϱ� �����մϴ�.
    /// </summary>
    protected override void GameInput_OnAttack2Started(object sender, EventArgs e)
    {
        spellManagerClientWizard.CastingNormalSpell(1);
    }
    /// <summary>
    /// Attack2 ��ư�� ������ �� ȣ��Ǵ� �޼����Դϴ�. ��ų�� �����մϴ�.
    /// </summary>
    protected override void GameInput_OnAttack2Ended(object sender, EventArgs e)
    {
        spellManagerClientWizard.ShootNormalSpell(1);
    }

    /// <summary>
    /// Attack3 ��ư�� ������ �� ȣ��Ǵ� �޼����Դϴ�. ��ų�� ĳ�����ϱ� �����մϴ�.
    /// </summary>
    protected override void GameInput_OnAttack3Started(object sender, EventArgs e)
    {
        spellManagerClientWizard.CastingBlizzard();
    }
    /// <summary>
    /// Attack3 ��ư�� ������ �� ȣ��Ǵ� �޼����Դϴ�. ��ų�� �����մϴ�.
    /// </summary>
    protected override void GameInput_OnAttack3Ended(object sender, EventArgs e)
    {
        spellManagerClientWizard.SetBlizzard();
    }

    /// <summary>
    /// Defence ��ư�� ������ �� ȣ��Ǵ� �޼����Դϴ�.
    /// Wizard�� Defence��ų�� ��� �ߵ��Ǳ� ������ ���⼭ �ٷ� ������ŵ�ϴ�.
    /// </summary>
    protected override void GameInput_OnDefenceStarted(object sender, EventArgs e)
    {
        spellManagerClientWizard.ActivateDefenceSpellOnClient();
    }
    /// <summary>
    /// Defence ��ư�� ������ �� ȣ��Ǵ� �޼����Դϴ�.
    /// Wizard�� Defence��ų�� ��� �ߵ��Ǳ� ������ ���ٸ� �۾��� ���� �ʽ��ϴ�.
    /// </summary>
    protected override void GameInput_OnDefenceEnded(object sender, EventArgs e) { }
    #endregion
}

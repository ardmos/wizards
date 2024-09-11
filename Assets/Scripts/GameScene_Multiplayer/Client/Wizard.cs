using System;
using Unity.Netcode;

/// <summary>
/// Wizard 캐릭터의 특성과 행동을 관리하는 클래스입니다.
/// PlayerClient를 상속받고 ICharacter 인터페이스를 구현합니다.
/// </summary>
public class Wizard : PlayerClient, ICharacter
{
    #region Fields and Properties
    /// <summary>
    /// Wizard 캐릭터의 스펠 매니저 컴포넌트입니다.
    /// </summary>
    public SpellManagerClientWizard spellManagerClientWizard;

    /// <summary>
    /// 캐릭터 클래스를 나타냅니다.
    /// </summary>
    public Character characterClass { get; set; } = Character.Wizard;
    /// <summary>
    /// 현재 체력을 나타냅니다.
    /// </summary>
    public sbyte hp { get; set; } = 5;
    /// <summary>
    /// 최대 체력을 나타냅니다.
    /// </summary>
    public sbyte maxHp { get; set; } = 5;
    /// <summary>
    /// 이동 속도를 나타냅니다.
    /// </summary>
    public float moveSpeed { get; set; } = 4f;
    /// <summary>
    /// Wizard가 사용할 수 있는 스킬 목록입니다.
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
    /// 플레이어를 초기화하는 ClientRpc 메서드입니다.
    /// </summary>
    [ClientRpc]
    public override void InitializePlayerClientRPC()
    {
        base.InitializePlayerClientRPC();
        // 보유 skill 정보를 GamePad UI에 반영          
        GameSceneUIManager.Instance.gamePadUIController.UpdateSpellUI(skills);
        // 보유 skill 정보를 바탕으로 스킬 매니저를 초기화
        spellManagerClientWizard.InitPlayerSpellInfoListClient(skills);
    }
    #endregion

    #region Character Data
    /// <summary>
    /// 캐릭터 데이터를 반환하는 메서드입니다.
    /// </summary>
    /// <returns>ICharacter 인터페이스를 구현한 현재 객체</returns>
    public ICharacter GetCharacterData()
    {
        return this;
    }
    #endregion

    #region Input Handling
    /// <summary>
    /// Attack1 버튼이 눌렸을 때 호출되는 메서드입니다. 스킬을 캐스팅하기 시작합니다.
    /// </summary>
    protected override void GameInput_OnAttack1Started(object sender, EventArgs e)
    {
        spellManagerClientWizard.CastingNormalSpell(0);
    }
    /// <summary>
    /// Attack1 버튼이 떼졌을 때 호출되는 메서드입니다. 스킬을 시전합니다.
    /// </summary>
    protected override void GameInput_OnAttack1Ended(object sender, EventArgs e)
    {
        spellManagerClientWizard.ShootNormalSpell(0);
    }

    /// <summary>
    /// Attack2 버튼이 눌렸을 때 호출되는 메서드입니다. 스킬을 캐스팅하기 시작합니다.
    /// </summary>
    protected override void GameInput_OnAttack2Started(object sender, EventArgs e)
    {
        spellManagerClientWizard.CastingNormalSpell(1);
    }
    /// <summary>
    /// Attack2 버튼이 떼졌을 때 호출되는 메서드입니다. 스킬을 시전합니다.
    /// </summary>
    protected override void GameInput_OnAttack2Ended(object sender, EventArgs e)
    {
        spellManagerClientWizard.ShootNormalSpell(1);
    }

    /// <summary>
    /// Attack3 버튼이 눌렸을 때 호출되는 메서드입니다. 스킬을 캐스팅하기 시작합니다.
    /// </summary>
    protected override void GameInput_OnAttack3Started(object sender, EventArgs e)
    {
        spellManagerClientWizard.CastingBlizzard();
    }
    /// <summary>
    /// Attack3 버튼이 떼졌을 때 호출되는 메서드입니다. 스킬을 시전합니다.
    /// </summary>
    protected override void GameInput_OnAttack3Ended(object sender, EventArgs e)
    {
        spellManagerClientWizard.SetBlizzard();
    }

    /// <summary>
    /// Defence 버튼이 눌렸을 때 호출되는 메서드입니다.
    /// Wizard의 Defence스킬은 즉시 발동되기 때문에 여기서 바로 시전시킵니다.
    /// </summary>
    protected override void GameInput_OnDefenceStarted(object sender, EventArgs e)
    {
        spellManagerClientWizard.ActivateDefenceSpellOnClient();
    }
    /// <summary>
    /// Defence 버튼이 떼졌을 때 호출되는 메서드입니다.
    /// Wizard의 Defence스킬은 즉시 발동되기 때문에 별다른 작업을 하지 않습니다.
    /// </summary>
    protected override void GameInput_OnDefenceEnded(object sender, EventArgs e) { }
    #endregion
}

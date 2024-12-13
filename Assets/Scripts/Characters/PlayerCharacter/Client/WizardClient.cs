using System;
using Unity.Netcode;

/// <summary>
/// 클라이언트 측에서 Wizard 캐릭터의 특성과 행동을 관리하는 클래스입니다.
/// PlayerClient를 상속받고 ICharacter 인터페이스를 구현합니다.
/// </summary>
public class WizardClient : PlayerClient
{
    #region Fields and Properties
    /// <summary>
    /// Wizard 캐릭터의 스펠 매니저 컴포넌트입니다.
    /// </summary>
    public SpellManagerClientWizard spellManagerClientWizard;
    #endregion

    #region Initialization
    [ClientRpc]
    public void InitializeWizardClientRPC(SpellName[] spellNames)
    {
        InitializePlayerClient();
        // 보유 마법 정보를 GamePad UI에 반영          
        GameSceneUIManager.Instance.gamePadUIController.UpdateSpellUI(spellNames);
        // 보유 마법 정보를 바탕으로 스킬 매니저를 초기화
        spellManagerClientWizard.InitPlayerSpellInfoListClient(spellNames);
    }
    #endregion

    #region Input Handling
    /// <summary>
    /// Attack1 버튼이 눌렸을 때 호출되는 메서드입니다. 스킬을 캐스팅하기 시작합니다.
    /// </summary>
    protected override void OnAttack_1_Started()
    {
        spellManagerClientWizard.CastingNormalSpell(0);
    }
    /// <summary>
    /// Attack1 버튼이 떼졌을 때 호출되는 메서드입니다. 스킬을 시전합니다.
    /// </summary>
    protected override void OnAttack_1_Ended()
    {
        spellManagerClientWizard.ReleaseNormalSpell(0);
    }

    /// <summary>
    /// Attack2 버튼이 눌렸을 때 호출되는 메서드입니다. 스킬을 캐스팅하기 시작합니다.
    /// </summary>
    protected override void OnAttack_2_Started()
    {
        spellManagerClientWizard.CastingNormalSpell(1);
    }
    /// <summary>
    /// Attack2 버튼이 떼졌을 때 호출되는 메서드입니다. 스킬을 시전합니다.
    /// </summary>
    protected override void OnAttack_2_Ended()
    {
        spellManagerClientWizard.ReleaseNormalSpell(1);
    }

    /// <summary>
    /// Attack3 버튼이 눌렸을 때 호출되는 메서드입니다. 스킬을 캐스팅하기 시작합니다.
    /// </summary>
    protected override void OnAttack_3_Started()
    {
        spellManagerClientWizard.CastingBlizzard();
    }
    /// <summary>
    /// Attack3 버튼이 떼졌을 때 호출되는 메서드입니다. 스킬을 시전합니다.
    /// </summary>
    protected override void OnAttack_3_Ended()
    {
        spellManagerClientWizard.ReleaseBlizzard();
    }

    /// <summary>
    /// Defence 버튼이 눌렸을 때 호출되는 메서드입니다.
    /// Wizard의 Defence스킬은 즉시 발동되기 때문에 여기서 바로 시전시킵니다.
    /// </summary>
    protected override void GameInput_OnDefenceStarted(object sender, EventArgs e)
    {
        spellManagerClientWizard.ActivateShield();
    }
    /// <summary>
    /// Defence 버튼이 떼졌을 때 호출되는 메서드입니다.
    /// Wizard의 Defence스킬은 즉시 발동되기 때문에 별다른 작업을 하지 않습니다.
    /// </summary>
    protected override void GameInput_OnDefenceEnded(object sender, EventArgs e) { }
    #endregion
}

using System;
using Unity.Netcode;
/// <summary>
/// 1. Knight 스탯 관리
/// </summary>
public class Knight : PlayerClient, ICharacter
{
    public SkillManagerClientKnight skillManagerClientKnight;
    public CharacterClass characterClass { get; set; } = CharacterClass.Knight;
    public sbyte hp { get; set; } = 10;
    public sbyte maxHp { get; set; } = 10;
    public float moveSpeed { get; set; } = 10f;
    public SkillName[] skills { get; set; } = new SkillName[]{
                SkillName.ElectricSlashAttack1_Lv1,
                SkillName.ElectricSlashAttack2_Lv1,
                SkillName.ElectricSlashAttack1_Lv1,
                SkillName.Dash_Lv1
                };

    [ClientRpc]
    public override void InitializePlayerClientRPC(SkillName[] skills)
    {
        base.InitializePlayerClientRPC(skills);
        // 보유 skill 정보를 클라이언트측 캐릭터의 스킬 컨트롤러에 전달.
        skillManagerClientKnight.InitPlayerSpellInfoListClient(skills);
    }

    public ICharacter GetCharacterData()
    {
        return this;
    }

    protected override void GameInput_OnAttack1Started(object sender, EventArgs e)
    {

    }

    protected override void GameInput_OnAttack1Ended(object sender, EventArgs e)
    {

    }

    protected override void GameInput_OnAttack2Started(object sender, EventArgs e)
    {

    }

    protected override void GameInput_OnAttack2Ended(object sender, EventArgs e)
    {

    }

    protected override void GameInput_OnAttack3Started(object sender, EventArgs e)
    {

    }

    protected override void GameInput_OnAttack3Ended(object sender, EventArgs e)
    {

    }

    protected override void GameInput_OnDefenceStarted(object sender, EventArgs e)
    {

    }

    protected override void GameInput_OnDefenceEnded(object sender, EventArgs e)
    {
       
    }
}

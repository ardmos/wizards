using System;
/// <summary>
/// 1. Knight ½ºÅÈ °ü¸®
/// </summary>
public class Knight : PlayerClient, ICharacter
{
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

/*    public void SetCharacterData(ICharacter characterData)
    {
        this.playerName = characterData.playerName;
        this.hp = characterData.hp;
        this.maxHp = characterData.maxHp;
        this.score = characterData.score;
        this.moveSpeed = characterData.moveSpeed;
        this.characterClass = characterData.characterClass;
        this.skills = characterData.skills;
    }*/

    public ICharacter GetCharacterData()
    {
        return this;
    }

    protected override void GameInput_OnAttack1Ended(object sender, EventArgs e)
    {
        throw new NotImplementedException();
    }

    protected override void GameInput_OnAttack1Started(object sender, EventArgs e)
    {
        throw new NotImplementedException();
    }

    protected override void GameInput_OnAttack2Ended(object sender, EventArgs e)
    {
        throw new NotImplementedException();
    }

    protected override void GameInput_OnAttack2Started(object sender, EventArgs e)
    {
        throw new NotImplementedException();
    }

    protected override void GameInput_OnAttack3Ended(object sender, EventArgs e)
    {
        throw new NotImplementedException();
    }

    protected override void GameInput_OnAttack3Started(object sender, EventArgs e)
    {
        throw new NotImplementedException();
    }

    protected override void GameInput_OnDefenceEnded(object sender, EventArgs e)
    {
        throw new NotImplementedException();
    }

    protected override void GameInput_OnDefenceStarted(object sender, EventArgs e)
    {
        throw new NotImplementedException();
    }
}

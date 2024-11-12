public static class CharacterSpecifications
{
    private static ICharacter Wizard = new WizardServer()
    {
        characterClass = Character.Wizard,
        hp = 5,
        maxHp = 5,
        moveSpeed = 5,
        spells = new SpellName[]{
                SpellName.FireBallLv1,
                SpellName.WaterBallLv1,
                SpellName.BlizzardLv1,
                SpellName.MagicShieldLv1
                }
    };
    private static ICharacter WizardAI = new WizardRukeAIServer()
    {
        characterClass = Character.Wizard,
        hp = 5,
        maxHp = 5,
        moveSpeed = 4,
        spells = new SpellName[]{
                SpellName.FireBallLv1,
                SpellName.WaterBallLv1,
                SpellName.BlizzardLv1,
                SpellName.MagicShieldLv1
                }
    };

    public static ICharacter GetCharacter(Character characterClass)
    {
        switch (characterClass)
        {
            case Character.Wizard:
                return Wizard;
            case Character.WizardAI: 
                return WizardAI;
            default: return Wizard;
        }
    }
    /*    public ICharacter Knight = new WizardServer()
        {
            characterClass = Character.Wizard,
            hp = 5,
            maxHp = 5,
            moveSpeed = 5,
            spells = new SpellName[]{
                    SpellName.FireBallLv1,
                    SpellName.WaterBallLv1,
                    SpellName.BlizzardLv1,
                    SpellName.MagicShieldLv1
                    }
        };*/
}

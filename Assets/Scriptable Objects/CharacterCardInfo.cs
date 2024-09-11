using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacterCardInfo", menuName = "ScriptableObjects/CharacterCardInfo", order = 0)]
public class CharacterCardInfo : ScriptableObject
{
    public Character character;
    public Sprite iconRole;
    public Sprite iconCharacter;
    public Sprite spriteBG;
    public string characterName;
    public string characterDescription;
    public int characterLevel;
    public List<SpellName> characterSkillSet = new List<SpellName>();
    public List<SkillInfoDetail> skillInfoDetails = new List<SkillInfoDetail>();
}

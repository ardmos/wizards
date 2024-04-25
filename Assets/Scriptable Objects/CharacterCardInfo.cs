using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class CharacterCardInfo : ScriptableObject
{
    public Character character;
    public Sprite iconRole;
    public Sprite iconCharacter;
    public Sprite spriteBG;
    public string characterName;
    public string characterDescription;
    public int characterLevel;
    public List<SkillName> characterSkillSet = new List<SkillName>();
}

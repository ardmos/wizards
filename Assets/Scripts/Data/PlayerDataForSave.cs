
/// <summary>
/// 플레이어 데이터 저장용 클래스. 현재는 로컬에 저장됩니다.
/// </summary>
[System.Serializable]
public class PlayerDataForSave 
{
    public ushort playerLevel;
    // Unity Auth에서 발급받은 playerId
    public string playerId;
    public string playerName;
    public CharacterClass characterClass;

    public PlayerDataForSave(PlayerData playerData)
    {
        playerLevel = playerData.playerLevel;
        playerId = playerData.playerId.ToString();
        playerName = playerData.playerName.ToString();
        characterClass = playerData.characterClass;
    }
}
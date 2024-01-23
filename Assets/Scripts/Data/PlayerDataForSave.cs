
/// <summary>
/// �÷��̾� ������ ����� Ŭ����. ����� ���ÿ� ����˴ϴ�.
/// </summary>
[System.Serializable]
public class PlayerDataForSave 
{
    public ushort playerLevel;
    // Unity Auth���� �߱޹��� playerId
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
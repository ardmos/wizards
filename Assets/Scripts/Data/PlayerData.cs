/// <summary>
/// �÷��̾� ������ Ŭ����. ����� ���ÿ� ����˴ϴ�.
/// </summary>
[System.Serializable]
public class PlayerData 
{
    public PlayerInGameData playerInGameData;
    public PlayerOutGameData playerOutGameData;

    public PlayerData(PlayerInGameData playerInGameData, PlayerOutGameData playerOutGameData)
    {
        this.playerInGameData = playerInGameData;
        this.playerOutGameData = playerOutGameData; 
    }

}
/// <summary>
/// 플레이어 데이터 클래스. 현재는 로컬에 저장됩니다.
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
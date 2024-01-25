using System;
/// <summary>
/// 유저 정보 클래스 입니다.
/// PlayerInGameData는 인게임에 사용되는 정보들을 담고있고, 이 클래스는 게임 밖에서 사용되는 정보들을 담고있습니다.
/// </summary>
[Serializable]
public class PlayerOutGameData 
{
    public byte hightestKOinOneMatch;
    public ushort mostWins;
    public ushort soloVictories;
    public uint knockOuts;
    public ulong totalScore;

    public byte playerLevel;

    public PlayerOutGameData()
    {
        hightestKOinOneMatch = 0;
        mostWins = 0;
        soloVictories = 0;
        knockOuts = 0;
        totalScore = 0;
        playerLevel = 1;
    }
}
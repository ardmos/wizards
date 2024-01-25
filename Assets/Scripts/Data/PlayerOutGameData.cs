using System;
/// <summary>
/// ���� ���� Ŭ���� �Դϴ�.
/// PlayerInGameData�� �ΰ��ӿ� ���Ǵ� �������� ����ְ�, �� Ŭ������ ���� �ۿ��� ���Ǵ� �������� ����ֽ��ϴ�.
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
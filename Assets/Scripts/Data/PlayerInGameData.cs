using System;
using Unity.Collections;
using Unity.Netcode;

[Serializable]
public struct PlayerInGameData : IEquatable<PlayerInGameData>, INetworkSerializable
{
    public ulong clientId;
    public PlayerMoveAnimState playerMoveAnimState;
    //public PlayerAttackAnimState playerAttackAnimState;
    public PlayerGameState playerGameState;
    public FixedString64Bytes playerName;
    public sbyte score;
    public Character characterClass;
    public sbyte hp;
    public sbyte maxHp;
    public float moveSpeed;
    public SkillName skillAttack1;
    public SkillName skillAttack2;
    public SkillName skillAttack3;
    public SkillName skillDefence;

    public bool Equals(PlayerInGameData other)
    {
        return
            clientId == other.clientId &&
            playerMoveAnimState == other.playerMoveAnimState &&
            //playerAttackAnimState == other.playerAttackAnimState &&
            playerGameState == other.playerGameState &&
            playerName == other.playerName &&
            hp == other.hp &&
            maxHp == other.maxHp &&
            score == other.score &&
            moveSpeed == other.moveSpeed &&
            characterClass == other.characterClass &&
            skillAttack1 == other.skillAttack1 &&
            skillAttack2 == other.skillAttack2 &&
            skillAttack3 == other.skillAttack3 &&
            skillDefence == other.skillDefence;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref clientId);
        serializer.SerializeValue(ref playerMoveAnimState);
        //serializer.SerializeValue(ref playerAttackAnimState);
        serializer.SerializeValue(ref playerGameState);
        serializer.SerializeValue(ref playerName);
        serializer.SerializeValue(ref hp);
        serializer.SerializeValue(ref maxHp);   
        serializer.SerializeValue(ref score);
        serializer.SerializeValue(ref moveSpeed);
        serializer.SerializeValue(ref characterClass);
        serializer.SerializeValue(ref skillAttack1);
        serializer.SerializeValue(ref skillAttack2);
        serializer.SerializeValue(ref skillAttack3);
        serializer.SerializeValue(ref skillDefence);
    }
}
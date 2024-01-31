using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;

/// <summary>
/// 인게임용 플레이어 정보 구조체. 클래스로 변경 가능한지 확인하기!  가능하다면 생성자를 만들어, 매개변수 없는 생성자의 경우 플레이어이름을 Player로 설정하도록 만든다. 
/// </summary>
[Serializable]
public struct PlayerInGameData : IEquatable<PlayerInGameData>, INetworkSerializable
{
    public ulong clientId;
    public FixedString64Bytes playerName;
    public CharacterClass characterClass;
    public sbyte playerHP;
    public sbyte playerMaxHP;
    public PlayerMoveAnimState playerMoveAnimState;
    public PlayerAttackAnimState playerAttackAnimState;
    public PlayerGameState playerGameState;

    public bool Equals(PlayerInGameData other)
    {
        return
            clientId == other.clientId &&
            playerName == other.playerName &&
            characterClass == other.characterClass &&
            playerHP == other.playerHP &&
            playerMaxHP == other.playerMaxHP &&
            playerMoveAnimState == other.playerMoveAnimState &&
            playerAttackAnimState == other.playerAttackAnimState &&
            playerGameState == other.playerGameState;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref clientId);
        serializer.SerializeValue(ref playerName);
        serializer.SerializeValue(ref characterClass);
        serializer.SerializeValue(ref playerHP);
        serializer.SerializeValue(ref playerMaxHP);
        serializer.SerializeValue(ref playerMoveAnimState);
        serializer.SerializeValue(ref playerAttackAnimState);
        serializer.SerializeValue(ref playerGameState);
    }
}

using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;

/// <summary>
/// ���� ���۽� ���Ǵ� �÷��̾� ������ ����ü. �ΰ��ӿ����ν� ����ü���� �����غ����� �ְڽ��ϴ�.
/// �ΰ��ӿ�/���ΰ��ӿ����� ������ Ŭ������ ��Ȯ�� ������ ���ؼ� ������. 
/// </summary>
public struct PlayerData : IEquatable<PlayerData>, INetworkSerializable
{
    public ulong clientId;
    public FixedString64Bytes playerName;
    public FixedString64Bytes playerId;
    public CharacterClass characterClass;
    public sbyte playerHP;
    public sbyte playerMaxHP;
    public PlayerMoveAnimState playerAnimState;
    public PlayerGameState playerGameState;
    public ushort playerLevel;

    public bool Equals(PlayerData other)
    {
        return
            clientId == other.clientId &&
            playerName == other.playerName &&
            playerId == other.playerId &&
            characterClass == other.characterClass &&
            playerHP == other.playerHP &&
            playerMaxHP == other.playerMaxHP &&
            playerAnimState == other.playerAnimState &&
            playerGameState == other.playerGameState &&
            playerLevel == other.playerLevel;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref clientId);
        serializer.SerializeValue(ref playerName);
        serializer.SerializeValue(ref playerId);
        serializer.SerializeValue(ref characterClass);
        serializer.SerializeValue(ref playerHP);
        serializer.SerializeValue(ref playerMaxHP);
        serializer.SerializeValue(ref playerAnimState);
        serializer.SerializeValue(ref playerGameState);
        serializer.SerializeValue(ref playerLevel);
    }
}

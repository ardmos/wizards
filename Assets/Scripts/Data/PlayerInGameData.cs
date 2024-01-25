using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;

/// <summary>
/// �ΰ��ӿ� �÷��̾� ���� ����ü. Ŭ������ ���� �������� Ȯ���ϱ�!  �����ϴٸ� �����ڸ� �����, �Ű����� ���� �������� ��� �÷��̾��̸��� Player�� �����ϵ��� �����. 
/// </summary>
[Serializable]
public struct PlayerInGameData : IEquatable<PlayerInGameData>, INetworkSerializable
{
    public ulong clientId;
    public FixedString64Bytes playerName;
    public CharacterClass characterClass;
    public sbyte playerHP;
    public sbyte playerMaxHP;
    public PlayerMoveAnimState playerAnimState;
    public PlayerGameState playerGameState;

    public bool Equals(PlayerInGameData other)
    {
        return
            clientId == other.clientId &&
            playerName == other.playerName &&
            characterClass == other.characterClass &&
            playerHP == other.playerHP &&
            playerMaxHP == other.playerMaxHP &&
            playerAnimState == other.playerAnimState &&
            playerGameState == other.playerGameState;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref clientId);
        serializer.SerializeValue(ref playerName);
        serializer.SerializeValue(ref characterClass);
        serializer.SerializeValue(ref playerHP);
        serializer.SerializeValue(ref playerMaxHP);
        serializer.SerializeValue(ref playerAnimState);
        serializer.SerializeValue(ref playerGameState);
    }
}

using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;

public struct PlayerData : IEquatable<PlayerData>, INetworkSerializable
{
    public ulong clientId;
    public FixedString64Bytes playerName;
    public FixedString64Bytes playerId;
    public CharacterClass playerClass;
    public sbyte playerHP;
    public PlayerMoveAnimState playerAnimState;
    public PlayerGameState playerGameState;

    public bool Equals(PlayerData other)
    {
        return
            clientId == other.clientId &&
            playerName == other.playerName &&
            playerId == other.playerId &&
            playerClass == other.playerClass &&
            playerHP == other.playerHP &&
            playerAnimState == other.playerAnimState &&
            playerGameState == other.playerGameState;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref clientId);
        serializer.SerializeValue(ref playerName);
        serializer.SerializeValue(ref playerId);
        serializer.SerializeValue(ref playerClass);
        serializer.SerializeValue(ref playerHP);
        serializer.SerializeValue(ref playerAnimState);
        serializer.SerializeValue(ref playerGameState);
    }
}

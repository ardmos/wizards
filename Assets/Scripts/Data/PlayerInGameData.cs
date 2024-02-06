using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;

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

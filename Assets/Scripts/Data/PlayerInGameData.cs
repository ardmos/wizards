using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;

[Serializable]
public struct PlayerInGameData : IEquatable<PlayerInGameData>, INetworkSerializable
{
    public ulong clientId;
    public PlayerCharacter characterData;
    public PlayerMoveAnimState playerMoveAnimState;
    public PlayerAttackAnimState playerAttackAnimState;
    public PlayerGameState playerGameState;

    public bool Equals(PlayerInGameData other)
    {
        return
            clientId == other.clientId &&
            characterData == other.characterData &&
            playerMoveAnimState == other.playerMoveAnimState &&
            playerAttackAnimState == other.playerAttackAnimState &&
            playerGameState == other.playerGameState;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref clientId);
        serializer.SerializeValue(ref characterData);
        serializer.SerializeValue(ref playerMoveAnimState);
        serializer.SerializeValue(ref playerAttackAnimState);
        serializer.SerializeValue(ref playerGameState);
    }
}

/// ICharacter를 곧바로 직렬화 할 수가 없어서 사용하는 클래스 <<<<<<< v피;ㄹ요한가???? 인터넷 포럼 확인부터! 그리고 필요없을지도모름!!!
public class PlayerCharacter : IEquatable<PlayerCharacter>, INetworkSerializable, ICharacter
{
    public FixedString64Bytes playerName { get; set; }
    public sbyte hp { get; set; }
    public sbyte maxHp { get; set; }
    public sbyte score { get; set; }
    public float moveSpeed { get; set; } 
    public CharacterClass characterClass { get; set; }
    public SkillName[] skills { get; set; }

    public bool Equals(PlayerCharacter other)
    {
        return
            playerName == other.playerName &&
            hp == other.hp &&
            maxHp == other.maxHp &&
            score == other.score &&
            moveSpeed == other.moveSpeed &&
            characterClass == other.characterClass &&
            skills == other.skills;
    }

    public ICharacter GetCharacterData()
    {
        return this;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        FixedString64Bytes playerNameString = playerName;
        serializer.SerializeValue(ref playerNameString);
        playerName = playerNameString;

        sbyte mhp = hp;
        serializer.SerializeValue(ref playerNameString);
        hp = mhp;

        sbyte mMaxHp = maxHp;
        serializer.SerializeValue(ref mMaxHp);
        maxHp = mMaxHp;

        sbyte mhp = hp;
        serializer.SerializeValue(ref playerNameString);
        hp = mhp;

        sbyte mhp = hp;
        serializer.SerializeValue(ref playerNameString);
        hp = mhp;

        sbyte mhp = hp;
        serializer.SerializeValue(ref playerNameString);
        hp = mhp;

        sbyte mhp = hp;
        serializer.SerializeValue(ref playerNameString);
        hp = mhp;
    }

    public void SetCharacterData(ICharacter characterData)
    {
        this.playerName = characterData.playerName;
        this.hp = characterData.hp;
        this.maxHp = characterData.maxHp;
        this.score = characterData.score;
        this.moveSpeed = characterData.moveSpeed;
        this.characterClass = characterData.characterClass;
        this.skills = characterData.skills;
    }
}
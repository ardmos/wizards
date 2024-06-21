using Unity.Netcode;
using UnityEngine;

[System.Serializable]
public class SpellInfo : INetworkSerializable
{
    public SpellType spellType;
    public SkillName spellName;
    public SpellState spellState;
    public float coolTime;
    public float lifeTime;
    public float moveSpeed;
    public int price;
    public byte level;
    public ulong ownerPlayerClientId;

    // for 업그레이드. UpgradeOption을 인덱스로 하는 옵션 배열입니다.
    public byte[] upgradeOptions;

    public SpellInfo() { }

    public SpellInfo(SpellInfo spellInfo) 
    {
        this.spellType = spellInfo.spellType;
        this.spellName = spellInfo.spellName;
        this.spellState = spellInfo.spellState;
        this.coolTime = spellInfo.coolTime;
        this.lifeTime = spellInfo.lifeTime;
        this.moveSpeed = spellInfo.moveSpeed;
        this.price = spellInfo.price;
        this.level = spellInfo.level;
        this.ownerPlayerClientId = spellInfo.ownerPlayerClientId;
        this.upgradeOptions = new byte[spellInfo.upgradeOptions.Length];
    }

    public SpellInfo(SpellType spellType, SkillName spellName, SpellState spellState, float coolTime, float lifeTime, float moveSpeed, int price, byte level, int upgradeOptionsCount)
    {
        this.spellType = spellType;
        this.spellName = spellName; 
        this.spellState = spellState;
        this.coolTime = coolTime;
        this.lifeTime = lifeTime;
        this.moveSpeed = moveSpeed;
        this.price = price;
        this.level = level;
        this.upgradeOptions = new byte[upgradeOptionsCount];
        //Debug.Log($"SpellInfo {spellName} 초기화. upgradeOptionsCount:{this.upgradeOptions.Length}");
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref spellType);
        serializer.SerializeValue(ref coolTime);
        serializer.SerializeValue(ref lifeTime);
        serializer.SerializeValue(ref moveSpeed);
        serializer.SerializeValue(ref price);
        serializer.SerializeValue(ref level);
        serializer.SerializeValue(ref spellName);
        serializer.SerializeValue(ref spellState);
        serializer.SerializeValue(ref ownerPlayerClientId);
    }
}

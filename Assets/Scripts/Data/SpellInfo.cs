using Unity.Netcode;

[System.Serializable]
public class SpellInfo : INetworkSerializable
{
    public SpellType spellType;
    public SpellName spellName;
    public SpellState spellState;
    public float coolTime;
    public float lifeTime;
    public float moveSpeed;
    public int price;
    public byte level;


    public ulong ownerPlayerClientId;

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
    }

    public SpellInfo(SpellType spellType, SpellName spellName, SpellState spellState, float coolTime, float lifeTime, float moveSpeed, int price, byte level)
    {
        this.spellType = spellType;
        this.spellName = spellName; 
        this.spellState = spellState;
        this.coolTime = coolTime;
        this.lifeTime = lifeTime;
        this.moveSpeed = moveSpeed;
        this.price = price;
        this.level = level;
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

    /// <summary>
    /// 스펠의 레벨에 맞는 이름을 설정해주는 메소드 입니다.
    /// </summary>
    /// <param name="level"></param>
    /// <param name="spellType"></param>
    public void SetSpellName(byte level, SpellType spellType)
    {
        switch(spellType)
        {
            case SpellType.Normal:
                // 기사 마법은 추후 수정 예정. 일단 테스트용.
                this.spellName = SpellName.SlashSpellStart + level;
                break;
            case SpellType.Fire:
                this.spellName = SpellName.FireSpellStart + level;
                break;

            case SpellType.Water:
                this.spellName = SpellName.WaterSpellStart + level;
                break;

            case SpellType.Ice:
                this.spellName = SpellName.IceSpellStart + level;
                break;

            default: break;
        }
    }
}

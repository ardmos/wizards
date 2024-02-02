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
    /// ������ ������ �´� �̸��� �������ִ� �޼ҵ� �Դϴ�.
    /// </summary>
    /// <param name="level"></param>
    /// <param name="spellType"></param>
    public void SetSpellName(byte level, SpellType spellType)
    {
        switch(spellType)
        {
            case SpellType.Normal:
                // ��� ������ ���� ���� ����. �ϴ� �׽�Ʈ��.
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

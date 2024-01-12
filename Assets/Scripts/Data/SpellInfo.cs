using Unity.Netcode;

[System.Serializable]
public class SpellInfo : INetworkSerializable
{
    public SpellType spellType;

    public float coolTime;
    public float lifeTime;
    public float moveSpeed;
    public int price;
    public sbyte level;
    public SpellName spellName;
    public SpellState spellState;

    public ulong ownerPlayerClientId;

    public bool isCollided;

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
        serializer.SerializeValue(ref isCollided);
    }

    /// <summary>
    /// ������ ���� Ÿ������ ���� �̸��� �˻��� �ο����ִ� �޼ҵ� �Դϴ�.
    /// </summary>
    /// <param name="level"></param>
    /// <param name="spellType"></param>
    public void SetSpellName(sbyte level, SpellType spellType)
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

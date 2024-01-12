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
    /// 레벨과 스펠 타입으로 스펠 이름을 검색해 부여해주는 메소드 입니다.
    /// </summary>
    /// <param name="level"></param>
    /// <param name="spellType"></param>
    public void SetSpellName(sbyte level, SpellType spellType)
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

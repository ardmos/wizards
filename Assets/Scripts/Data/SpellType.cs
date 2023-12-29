using Unity.Netcode;

public enum SpellType 
{
    Normal,
    Fire,
    Water,
    Ice,
    Lightning,
    Arcane
}

// Spell Info로 대체. 현재는 안쓰는 구조체. 삭제 가능
/*public struct SpellLvlType : INetworkSerializable
{
    public SpellType spellType;
    public int level;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref spellType);
        serializer.SerializeValue(ref level);
    }
}*/
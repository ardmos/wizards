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

public struct SpellLvlType : INetworkSerializable
{
    public SpellType spellType;
    public int level;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref spellType);
        serializer.SerializeValue(ref level);
    }
}
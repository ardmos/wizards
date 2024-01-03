using UnityEngine;
using Unity.Netcode;

public class SpellInfo : INetworkSerializable
{
    public SpellType spellType;

    public float coolTime;
    public float lifeTime;
    public float moveSpeed;
    public int price;
    public int level;
    public SpellName spellName;
    public SpellState spellState;

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

    }
}

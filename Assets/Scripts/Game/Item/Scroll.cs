using Unity.Netcode;

[System.Serializable]
public class Scroll : NetworkBehaviour, INetworkSerializable
{
    public Item.ItemName itemName;

    public virtual void ApplyScroll(SpellInfo spellInfo)
    {
        
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref itemName);
    }
}
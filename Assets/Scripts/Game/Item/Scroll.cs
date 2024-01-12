using Unity.Netcode;

[System.Serializable]
public class Scroll : NetworkBehaviour, INetworkSerializable
{
    public Item.ItemName scrollName;

    public virtual void UpdateScrollEffectToServer(sbyte spellIndex) { }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref scrollName);
    }
}
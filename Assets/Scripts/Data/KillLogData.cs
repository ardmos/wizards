using System;
using Unity.Collections;
using Unity.Netcode;
/// <summary>
/// ���� ų�α� ���׷��̵��� �� ��밡���� ����ü. ������ ���� �÷��̾� �̸��� ���� ����. ��� ����. ���� ����.
/// </summary>
public struct KillLogData : IEquatable<KillLogData>, INetworkSerializable
{
    public FixedString64Bytes gotTheGameOverplayerName;
    public FixedString64Bytes gaveTheGameOverPlayerName;

    public KillLogData(FixedString64Bytes gotTheGameOverplayerName, FixedString64Bytes gaveTheGameOverPlayerName) : this()
    {
        this.gotTheGameOverplayerName = gotTheGameOverplayerName;
        this.gaveTheGameOverPlayerName = gaveTheGameOverPlayerName;
    }

    public bool Equals(KillLogData other)
    {
        return
            gotTheGameOverplayerName == other.gotTheGameOverplayerName &&
            gaveTheGameOverPlayerName == other.gaveTheGameOverPlayerName;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref gotTheGameOverplayerName);
        serializer.SerializeValue(ref gaveTheGameOverPlayerName);
    }
}

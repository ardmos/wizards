using System;
using Unity.Collections;
using Unity.Netcode;
/// <summary>
/// 추후 킬로그 업그레이드할 때 사용가능한 구조체. 지금은 죽은 플레이어 이름만 띄우고 있음. 사용 안함. 삭제 가능.
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

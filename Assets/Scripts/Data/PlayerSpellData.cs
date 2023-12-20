using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
/// <summary>
/// ��� ���� ��Ȳ�� ��� �����Ұ����� �׽�Ʈ��. ���� ����
/// </summary>
public struct WizardSpell : IEquatable<WizardSpell>, INetworkSerializable
{
    public SpellNames.Wizard spellName;

    public bool Equals(WizardSpell other)
    {
        return
            spellName == other.spellName;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        if (serializer.IsWriter)
        {
            serializer.GetFastBufferWriter().WriteValueSafe(spellName);
        }
        else
        {
            serializer.GetFastBufferReader().ReadValueSafe(out spellName);
        }
    }
}
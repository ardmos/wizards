using System;

class SpellStateEventData : EventArgs
{
    public ulong clientId;
    public ushort spellIndex;

    public SpellStateEventData(ulong clientId, ushort spellIndex)
    {
        this.clientId = clientId;
        this.spellIndex = spellIndex;
    }
}
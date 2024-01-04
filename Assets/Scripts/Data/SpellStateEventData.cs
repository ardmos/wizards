using System;

class SpellStateEventData : EventArgs
{
    public ulong clientId;
    public SpellState spellState;

    public SpellStateEventData(ulong clientId, SpellState spellState)
    {
        this.clientId = clientId;
        this.spellState = spellState;
    }
}
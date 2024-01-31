using System;

class PlayerAttackAnimStateEventData : EventArgs
{
    public ulong clientId;
    public PlayerAttackAnimState playerAttackAnimState;

    public PlayerAttackAnimStateEventData(ulong clientId, PlayerAttackAnimState playerAttackAnimState)
    {
        this.clientId = clientId;
        this.playerAttackAnimState = playerAttackAnimState;
    }
}
using System;

class PlayerAnimStateEventData : EventArgs
{
    public ulong clientId;
    public PlayerMoveAnimState playerMoveAnimState;

    public PlayerAnimStateEventData(ulong clientId, PlayerMoveAnimState playerMoveAnimState)
    {
        this.clientId = clientId;
        this.playerMoveAnimState = playerMoveAnimState;
    }
}
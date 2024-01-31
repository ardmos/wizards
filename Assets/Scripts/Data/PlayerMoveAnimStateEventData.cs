using System;

class PlayerMoveAnimStateEventData : EventArgs
{
    public ulong clientId;
    public PlayerMoveAnimState playerMoveAnimState;

    public PlayerMoveAnimStateEventData(ulong clientId, PlayerMoveAnimState playerMoveAnimState)
    {
        this.clientId = clientId;
        this.playerMoveAnimState = playerMoveAnimState;
    }
}
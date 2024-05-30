using System;

public class PlayerGameOverEventArgs : EventArgs
{
    public ulong clientIDWhoGameOver;
    public ulong clientIDWhoAttacked;
}
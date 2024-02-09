using Unity.Netcode;
using UnityEngine;

public class ScrollLevelUp : Scroll
{
    public override void OnNetworkSpawn()
    {
        scrollName = ItemName.Scroll_LevelUp;
    }
}

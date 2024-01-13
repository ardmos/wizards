using Unity.Netcode;
using UnityEngine;

public class ScrollLevelUp : Scroll
{
    public override void OnNetworkSpawn()
    {
        scrollName = Item.ItemName.Scroll_LevelUp;
    }
}

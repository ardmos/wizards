using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 상점에 현재 방문객 등록을 위한 인터페이스. 상점 손님으로써 필요한 기능들을 갖고있습니다.
/// </summary>

public interface IStoreCustomer
{
    void BoughtSpellScroll(Item.ItemType itemType, int slotNum);
    void BoughtItem(Item.ItemType itemType);
}

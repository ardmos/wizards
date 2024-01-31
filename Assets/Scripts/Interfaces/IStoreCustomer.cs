using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 현재 사용 안하는 스크립트. 추후에 인게임 상점 추가시 사용가능성 있음.
/// 
/// 상점에 현재 방문객 등록을 위한 인터페이스. 상점 손님으로써 필요한 기능들을 갖고있습니다.
/// </summary>

public interface IStoreCustomer
{
    void BoughtSpellScroll(Item.ItemName itemType, int slotNum);
    void BoughtItem(Item.ItemName itemType);
}

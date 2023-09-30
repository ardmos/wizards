using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 상점에 현재 방문객 등록을 위한 인터페이스. 상점 손님으로써 필요한 기능들을 갖고있습니다.
/// </summary>

public interface IStoreCustomer
{
    // Item 클래스, GameAssets 클래스 만들어둘 필요가 있음. 
    void BoughtItem(Item.ItemType itemType);
}

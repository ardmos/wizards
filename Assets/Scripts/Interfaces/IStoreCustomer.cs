using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// ������ ���� �湮�� ����� ���� �������̽�. ���� �մ����ν� �ʿ��� ��ɵ��� �����ֽ��ϴ�.
/// </summary>

public interface IStoreCustomer
{
    // Item Ŭ����, GameAssets Ŭ���� ������ �ʿ䰡 ����. 
    void BoughtItem(Item.ItemType itemType);
}

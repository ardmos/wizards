using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// ������ ���� �湮�� ����� ���� �������̽�. ���� �մ����ν� �ʿ��� ��ɵ��� �����ֽ��ϴ�.
/// </summary>

public interface IStoreCustomer
{
    void BoughtSpellScroll(Item.ItemName itemType, int slotNum);
    void BoughtItem(Item.ItemName itemType);
}

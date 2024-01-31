using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// ���� ��� ���ϴ� ��ũ��Ʈ. ���Ŀ� �ΰ��� ���� �߰��� ��밡�ɼ� ����.
/// 
/// ������ ���� �湮�� ����� ���� �������̽�. ���� �մ����ν� �ʿ��� ��ɵ��� �����ֽ��ϴ�.
/// </summary>

public interface IStoreCustomer
{
    void BoughtSpellScroll(Item.ItemName itemType, int slotNum);
    void BoughtItem(Item.ItemName itemType);
}

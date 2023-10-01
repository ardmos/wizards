using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// ���������� UI�� �����ϴ� ��ũ��Ʈ.
/// !!! ���� ���
///     1. ���� UIȰ��ȭ/��Ȱ��ȭ
///     2. ������ ������ ���� (���ø� Ȱ��)
///     3. ������ ����
/// </summary>
public class UI_MagicStore : MonoBehaviour
{
    [SerializeField]
    private Transform container;
    [SerializeField]
    private Transform storeItemTemplatePref;

    private IStoreCustomer storeCustomer;

    // Start is called before the first frame update
    void Start()
    {
        // �⺻ ��Ȱ��ȭ ����.
        // ��Ȱ��ȭ ������ ���� ǰ�� �����ʹ� �ε尡 ��������
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// ���� ������ �������� �����ϴ� �޼���
    /// 1. ������ ��� ������ ����
    /// 2. �����Ǵ� ������ �����ۿ� �� ItemType�� �� ��ũ��Ʈ �ּҸ� �־��� (�� ������ Ŭ���� Ȱ���� �� �ֵ���)
    /// </summary>
    public void CreateItembutton()
    {
        for (int i = ((int)Item.ItemType.Wand_1); i < ((int)Item.ItemType.Max); i++)
        {
            Item.ItemType itemType = (Item.ItemType)i;
            Transform storeItemTransform = Instantiate(storeItemTemplatePref);
            storeItemTransform.SetParent(container);
            UI_MagicStoreItemTemplate ui_MagicStoreItem = storeItemTransform.GetComponent<UI_MagicStoreItemTemplate>();
            ui_MagicStoreItem.InitItemInfo(itemType.ToString(), Item.GetCost(itemType).ToString());
            ui_MagicStoreItem.itemType = itemType;
            ui_MagicStoreItem.magicStore = this;
        }
    }

    /// <summary>
    /// ������ Ȱ��ȭ ��Ű�� �޼��� 
    /// </summary>
    public void Show(IStoreCustomer storeCustomer)
    {
        this.storeCustomer = storeCustomer;
        Debug.Log("ON");
        gameObject.SetActive(true);
        CreateItembutton();
    }
    /// <summary>
    /// ������ ��Ȱ��ȭ ��Ű�� �޼���
    /// </summary>
    public void Hide()
    {
        Debug.Log("OFF");
        gameObject.SetActive(false);
    }

    public void TryBuyItem(Item.ItemType itemType)
    {
        storeCustomer.BoughtItem(itemType);
    }
}

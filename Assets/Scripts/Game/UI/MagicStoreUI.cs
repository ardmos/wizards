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
public class MagicStoreUI : MonoBehaviour
{
    [SerializeField] private Transform container;
    [SerializeField] private Transform storeItemTemplatePref;
    [SerializeField] private SelectSpellSlotUI selectSpellSlotPopup;

    private IStoreCustomer storeCustomer;

    private void Awake()
    {
        // ���� ǰ��� �ε�
        LoadContents();
    }
    // Start is called before the first frame update
    void Start()
    {
        // ���� UI ��Ȱ��ȭ
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
    public void LoadContents()
    {
        for (int i = ((int)Item.ItemType.FireBall_1); i < ((int)Item.ItemType.Max); i++)
        {
            Item.ItemType itemType = (Item.ItemType)i;
            Transform storeItemTransform = Instantiate(storeItemTemplatePref);
            storeItemTransform.SetParent(container);
            MagicStoreItemTemplateUI ui_MagicStoreItem = storeItemTransform.GetComponent<MagicStoreItemTemplateUI>();
            ui_MagicStoreItem.InitItemInfo(itemType.ToString(), Item.GetCost(itemType).ToString());
            ui_MagicStoreItem.itemType = itemType;
            ui_MagicStoreItem.selectSpellSlotPopup = selectSpellSlotPopup;
        }
    }

    /// <summary>
    /// ������ Ȱ��ȭ ��Ű�� �޼��� 
    /// </summary>
    public void Show(IStoreCustomer storeCustomer)
    {
        gameObject.SetActive(true);
        this.storeCustomer = storeCustomer;
    }
    /// <summary>
    /// ������ ��Ȱ��ȭ ��Ű�� �޼���
    /// </summary>
    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void TryBuyItem(Item.ItemType itemType, int slotNum)
    {
        storeCustomer.BoughtSpellScroll(itemType, slotNum);
        selectSpellSlotPopup.Hide();
    }
}
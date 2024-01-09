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
public class PopupMagicStoreUI : MonoBehaviour
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
        Hide();
    }

    /// <summary>
    /// ���� ������ �������� �����ϴ� �޼���
    /// 1. ������ ��� ������ ����
    /// 2. �����Ǵ� ������ �����ۿ� �� ItemType�� �� ��ũ��Ʈ �ּҸ� �־��� (�� ������ Ŭ���� Ȱ���� �� �ֵ���)
    /// </summary>
    public void LoadContents()
    {
        for (int i = ((int)Item.ItemName.SpellStart+1); i < ((int)Item.ItemName.SpellEnd); i++)
        {
            Item.ItemName itemType = (Item.ItemName)i;
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

    /// <summary>
    /// �������� �����ϴ� �޼���
    /// </summary>
    /// <param name="itemType"></param>
    /// <param name="slotNum"></param>
    public void TryBuyItem(Item.ItemName itemType, int slotNum)
    {
        storeCustomer.BoughtSpellScroll(itemType, slotNum);
        selectSpellSlotPopup.Hide();
    }
}

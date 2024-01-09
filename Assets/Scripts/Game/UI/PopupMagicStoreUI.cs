using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 매직상점의 UI를 관리하는 스크립트.
/// !!! 현재 기능
///     1. 상점 UI활성화/비활성화
///     2. 컨텐츠 아이템 생성 (템플릿 활용)
///     3. 아이템 구매
/// </summary>
public class PopupMagicStoreUI : MonoBehaviour
{
    [SerializeField] private Transform container;
    [SerializeField] private Transform storeItemTemplatePref;
    [SerializeField] private SelectSpellSlotUI selectSpellSlotPopup;

    private IStoreCustomer storeCustomer;

    private void Awake()
    {
        // 상점 품목들 로드
        LoadContents();
    }
    // Start is called before the first frame update
    void Start()
    {
        // 상점 UI 비활성화
        Hide();
    }

    /// <summary>
    /// 상점 아이템 컨텐츠를 생성하는 메서드
    /// 1. 프리팹 사용 아이템 생성
    /// 2. 생성되는 각각의 아이템에 각 ItemType과 현 스크립트 주소를 넣어줌 (각 아이템 클릭시 활용할 수 있도록)
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
    /// 상점을 활성화 시키는 메서드 
    /// </summary>
    public void Show(IStoreCustomer storeCustomer)
    {
        gameObject.SetActive(true);
        this.storeCustomer = storeCustomer;
    }
    /// <summary>
    /// 상점을 비활성화 시키는 메서드
    /// </summary>
    public void Hide()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 아이템을 구매하는 메서드
    /// </summary>
    /// <param name="itemType"></param>
    /// <param name="slotNum"></param>
    public void TryBuyItem(Item.ItemName itemType, int slotNum)
    {
        storeCustomer.BoughtSpellScroll(itemType, slotNum);
        selectSpellSlotPopup.Hide();
    }
}

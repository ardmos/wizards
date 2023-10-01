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
        // 기본 비활성화 상태.
        // 비활성화 이전에 상점 품목 데이터는 로드가 끝나야함
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// 상점 아이템 컨텐츠를 생성하는 메서드
    /// 1. 프리팹 사용 아이템 생성
    /// 2. 생성되는 각각의 아이템에 각 ItemType과 현 스크립트 주소를 넣어줌 (각 아이템 클릭시 활용할 수 있도록)
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
    /// 상점을 활성화 시키는 메서드 
    /// </summary>
    public void Show(IStoreCustomer storeCustomer)
    {
        this.storeCustomer = storeCustomer;
        Debug.Log("ON");
        gameObject.SetActive(true);
        CreateItembutton();
    }
    /// <summary>
    /// 상점을 비활성화 시키는 메서드
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

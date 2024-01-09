using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 게임씬 승리 플레이어에게 노출되는 UI
/// 1. 게임동안 얻은 배틀패스 경험치(한 판당 10) 슬라이더에 뜨워주기.(슬라이더가 차오르는것이 보이게끔 구현)
/// 2. 얻은 아이템 띄워주기. (하나씩 애니메이션을 보여주며 생성)
/// 3. 'Claim' 클릭시 모든 상품 수령 & 로비씬 이동
/// 4. 'Claim 2배' 클릭시 광고영상 재생
///     1. 광고 시청 실패시 원상복귀
///     2. 광고 시청 완료시 모든 상품 2배 수령(배틀패스 포함) & 로비씬 이동
///     
/// 팝업에서 동작하는 애니메이션 목록
/// Step1. Victory 문구 최초 중앙 등장 이후 상승 & Detail영역 x축 scale값 상승 애니메이션(자동 실행)
/// Step2. Step1 이후 배틀패스 슬라이더 값 차오르는 애니메이션 & 얻은 아이템들 순서대로 또잉또잉 등장
/// </summary>
public class PopupWinUI : MonoBehaviour
{
    // 테스트용 보상 아이템 리스트 목록 하드코딩. 따로 구현할 필요 있음.
    [SerializeField] private Item.ItemType[] rewardItems = new Item.ItemType[4] {
        Item.ItemType.Item_BonusGold,
        Item.ItemType.Item_Exp,
        Item.ItemType.Item_Wizard,
        Item.ItemType.Item_Knight
    };

    [SerializeField] private Animator animator;
    [SerializeField] private Slider sliderBattlePath;
    [SerializeField] private GameObject detailArea;
    [SerializeField] private GameObject btnClaim;
    [SerializeField] private GameObject btnClaim2x;
    [SerializeField] private GameObject imgEffect;
   
    // 아래 itemTemplate들 GameAssets에 저장 안해도 되는지?
    [SerializeField] private GameObject itemTemplateGray;
    [SerializeField] private GameObject itemTemplateBlue;
    [SerializeField] private GameObject itemTemplateYellow;
    [SerializeField] private Transform itemGroup;

    // Start is called before the first frame update
    void Start()
    {
        sliderBattlePath.gameObject.SetActive(false);
        imgEffect.SetActive(false);
        btnClaim.SetActive(false);
        btnClaim2x.SetActive(false);

        btnClaim.GetComponent<Button>().onClick.AddListener(() =>
        {

        });
        btnClaim2x.GetComponent<Button>().onClick.AddListener(() =>
        {

        });
    }

    /// <summary>
    /// Step2. 배틀패스 슬라이더 값 차오르는 애니메이션 & 얻은 아이템들 순서대로 또잉또잉 등장 & 꽃가루 등장 & 버튼 등장
    /// Step1. 애니메이션 끝부분에서 AnimationEvent로 호출해주고있는 메소드 입니다.
    /// </summary>
    public void ShowResultDetails()
    {
        StartCoroutine(StartResultDetailsAnim());
    }

    /// <summary>
    /// 팝업에 상세정보를 띄워주는 메소드 입니다.
    /// </summary>
    private IEnumerator StartResultDetailsAnim()
    {
        sliderBattlePath.gameObject.SetActive(true);
        // 1. 배틀패스 슬라이더값 차오르는 애니메이션 (테스트용으로 value 0% -> 10% 까지 채워주기. 배틀패스 추가 후에는 해당 값으로 채워주기)
        yield return StartCoroutine(FillSliderValue(10f));
        // 2. 얻은 아이템들 순서대로 등장
        yield return StartCoroutine(LoadEarnedItems());
        // 3. 꽃가루 등장
        imgEffect.SetActive(true);
        // 4. 버튼들 등장
        yield return new WaitForSeconds(0.4f);
        btnClaim.SetActive(true);
        yield return new WaitForSeconds(0.4f);
        btnClaim2x.SetActive(true);
    }

    private IEnumerator FillSliderValue(float maxValue)
    {
        float value = 0f;
        
        while (value <= maxValue)
        {
            Debug.Log($"value : {value}");
            sliderBattlePath.value = value;
            sliderBattlePath.GetComponentInChildren<TextMeshProUGUI>().text = $"{value} <#9aa5d1>/ 100";
            yield return new WaitForSeconds(0.05f);
            value += 1f;
        }
    }

    private IEnumerator LoadEarnedItems()
    {
        foreach (Item.ItemType item in rewardItems)
        {
            yield return new WaitForSeconds(0.5f);

            GameObject templateObject = null;
            switch (item)
            {
                case Item.ItemType.Item_BonusGold:
                    templateObject = itemTemplateBlue;
                    break;
                case Item.ItemType.Item_Exp:
                    templateObject = itemTemplateYellow;
                    break;
                default:
                    templateObject = itemTemplateGray;
                    break;
            }

            GameObject itemObject = Instantiate(templateObject);
            itemObject.GetComponent<WinPopupItemTemplate>().InitTemplate(Item.GetIcon(item), "1", true);
            itemObject.transform.SetParent(itemGroup, false);           
        }
    }
}

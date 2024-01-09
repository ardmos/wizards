using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// ���Ӿ� �¸� �÷��̾�� ����Ǵ� UI
/// 1. ���ӵ��� ���� ��Ʋ�н� ����ġ(�� �Ǵ� 10) �����̴��� �߿��ֱ�.(�����̴��� �������°��� ���̰Բ� ����)
/// 2. ���� ������ ����ֱ�. (�ϳ��� �ִϸ��̼��� �����ָ� ����)
/// 3. 'Claim' Ŭ���� ��� ��ǰ ���� & �κ�� �̵�
/// 4. 'Claim 2��' Ŭ���� ������ ���
///     1. ���� ��û ���н� ���󺹱�
///     2. ���� ��û �Ϸ�� ��� ��ǰ 2�� ����(��Ʋ�н� ����) & �κ�� �̵�
///     
/// �˾����� �����ϴ� �ִϸ��̼� ���
/// Step1. Victory ���� ���� �߾� ���� ���� ��� & Detail���� x�� scale�� ��� �ִϸ��̼�(�ڵ� ����)
/// Step2. Step1 ���� ��Ʋ�н� �����̴� �� �������� �ִϸ��̼� & ���� �����۵� ������� ���׶��� ����
/// </summary>
public class PopupWinUI : MonoBehaviour
{
    // �׽�Ʈ�� ���� ������ ����Ʈ
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
   
    // �Ʒ� itemTemplate�� GameAssets�� ���� ���ص� �Ǵ���?
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
    }


    /// <summary>
    /// Step2. ��Ʋ�н� �����̴� �� �������� �ִϸ��̼� & ���� �����۵� ������� ���׶��� ���� & �ɰ��� ���� & ��ư ����
    /// </summary>
    public void ShowResultDetails()
    {
        // �ɰ��� ����
        imgEffect.SetActive(true);
        // ��Ʋ�н� �����̴��� �������� �ִϸ��̼� & ���� �����۵� ������� �����ֱ� & ��ư ����
        StartResultDetailsAnim();
    }

    /// <summary>
    /// ��Ʋ�н� �����̴� �������� �ִϸ��̼� ���� �޼ҵ� �׽�Ʈ��.(���Ŀ� ��Ʋ�н� �߰��� �̰��� ����)
    /// </summary>
    private void StartResultDetailsAnim()
    {
        sliderBattlePath.gameObject.SetActive(true);
        // �׽�Ʈ������ value 0% -> 10% ���� ä���ֱ�. ��Ʋ�н� �߰� �Ŀ��� �ش� ������ ä���ֱ�  
        StartCoroutine(FillSliderValue(10f));
    }

    private void ShowEarnedItems()
    {
        StartCoroutine(LoadEarnedItems());
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
        // ������ ��ư�� SetActive true
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

        // ���� �����۵� ������� �����ֱ�
        ShowEarnedItems();
    }
}

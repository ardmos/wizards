using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
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
public class PopupWinUIController : MonoBehaviour
{
    // �׽�Ʈ�� ���� ������ ����Ʈ ��� �ϵ��ڵ�. ���� ������ �ʿ� ����.
    [SerializeField] private Dictionary<ItemName, ushort> rewardItems = new Dictionary<ItemName, ushort>() {
        { ItemName.Item_BonusGold, 7 },
        { ItemName.Item_Exp, 25 },
        { ItemName.Item_Wizard, 2 },
        { ItemName.Item_Knight, 1 }
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

    private WaitForSeconds waitForSeconds = new WaitForSeconds(0.4f);

    // Start is called before the first frame update
    void Start()
    {
        sliderBattlePath.gameObject.SetActive(false);
        imgEffect.SetActive(false);
        btnClaim.SetActive(false);
        btnClaim2x.SetActive(false);

        // ��� ��ǰ ���� & �κ�� �̵�
        btnClaim.GetComponent<CustomClickSoundButton>().AddClickListener(() =>
        {
            // ���� ����. ���� �ܰ�! ���ɹ�ư Ŭ�� ��� ������ ����.
            // 1. Ŭ���̾�Ʈ�� �� btnClaim Ŭ�� & �������� clientId�� itemName ����.
            // 2. ServerRPC ���� ���޹���. playerItemDictionary�� ����.
            GameMultiplayer.Instance.AddPlayerItemServerRPC(rewardItems.Keys.ToArray<ItemName>(), rewardItems.Values.ToArray<ushort>());

            // NetworkManager ����
            CleanUp();
            // �κ�� �̵�. 
            LoadSceneManager.Load(LoadSceneManager.Scene.LobbyScene);
        });

        // Ŭ���� ������ ��� ( �̱��� )
        btnClaim2x.GetComponent<CustomClickSoundButton>().AddClickListener(() =>
        {
            // 1. ���� ��û ���н� ���󺹱�
            // 2. ���� ��û �Ϸ�� ��� ��ǰ 2�� ����(��Ʋ�н� ����) & �κ�� �̵�
        });

        Hide();
    }

    public void Show()
    {
        //Debug.Log($"Win Popup Show()");
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Step2. ��Ʋ�н� �����̴� �� �������� �ִϸ��̼� & ���� �����۵� ������� ���׶��� ���� & �ɰ��� ���� & ��ư ����
    /// Step1. �ִϸ��̼� ���κп��� AnimationEvent�� ȣ�����ְ��ִ� �޼ҵ� �Դϴ�.
    /// </summary>
    public void ShowResultDetails()
    {
        StartCoroutine(ShowDetails());
    }

    /// <summary>
    /// �˾��� �������� ����ִ� �޼ҵ� �Դϴ�.
    /// </summary>
    private IEnumerator ShowDetails()
    {
        //sliderBattlePath.gameObject.SetActive(true);
        // 1. ��Ʋ�н� �����̴��� �������� �ִϸ��̼� (�׽�Ʈ������ value 0% -> 10% ���� ä���ֱ�. ��Ʋ�н� �߰� �Ŀ��� �ش� ������ ä���ֱ�)
        //yield return StartCoroutine(FillSliderValue(10f));
        // 2. ���� �����۵� ������� ����
        //yield return StartCoroutine(LoadEarnedItems());
        // 3. �ɰ��� ����
        imgEffect.SetActive(true);
        // 4. ��ư�� ����
        yield return waitForSeconds;
        btnClaim.SetActive(true);
        yield return waitForSeconds;
        //btnClaim2x.SetActive(true); //���� ���� �� ����
    }

    private IEnumerator FillSliderValue(float maxValue)
    {
        float value = 0f;
        
        while (value <= maxValue)
        {
            //Debug.Log($"value : {value}");
            sliderBattlePath.value = value;
            sliderBattlePath.GetComponentInChildren<TextMeshProUGUI>().text = $"{value} <#9aa5d1>/ 100";
            yield return new WaitForSeconds(0.05f);
            value += 1f;
        }
    }

    private IEnumerator LoadEarnedItems()
    {
        foreach (KeyValuePair<ItemName, ushort> item in rewardItems)
        {
            yield return waitForSeconds;

            GameObject templateObject = null;
            switch (item.Key)
            {
                case ItemName.Item_BonusGold:
                    templateObject = itemTemplateBlue;
                    break;
                case ItemName.Item_Exp:
                    templateObject = itemTemplateYellow;
                    break;
                default:
                    templateObject = itemTemplateGray;
                    break;
            }

            GameObject itemObject = Instantiate(templateObject);
            itemObject.GetComponent<WinPopupItemTemplateUI>().InitTemplate(Item.GetIcon(item.Key), item.Value.ToString(), true);
            itemObject.transform.SetParent(itemGroup, false);           
        }
    }

    private void CleanUp()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
            Destroy(NetworkManager.Singleton.gameObject);
        }
        if (GameMultiplayer.Instance != null)
        {
            Destroy(GameMultiplayer.Instance.gameObject);
        }
    }
}

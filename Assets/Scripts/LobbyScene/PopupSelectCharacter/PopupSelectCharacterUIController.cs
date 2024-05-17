using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 1. Select(���� ����) ��ư
/// 2. Select(���� ����) ī�常 ��Ŀ�� ȿ�� ����
/// 3. ���� Ŭ��(�̸����� ����)�� ī�� ĳ���� ���� �߾ӿ� ǥ�� ( �ϴ��� ĳ���� ������ ĳ���� ī�尡 ������ �ִ�. )
/// 4. ���ư��� ��ư
/// </summary>
public class PopupSelectCharacterUIController : MonoBehaviour
{
    public LobbySceneUIController lobbySceneUIController;
    public CustomClickSoundButton btnBack;
    public CustomClickSoundButton btnSelectCharacter;

    public Transform containerCharacterCards;
    public GameObject prefabCharacterCard;

    [Header("ȭ�鿡 ������ ĳ���� ī�� ������")]
    public CharacterCardInfo[] characterCardInfos;

    [Header("���õ� ĳ������ ������ ǥ���� UI ������Ʈ��")]
    public Image imgIconRole;
    public TextMeshProUGUI txtCharacterName;
    public TextMeshProUGUI txtCharacterDescription;
    public Image imgIconSkill1;
    public Image imgIconSkill2;
    public Image imgIconSkill3;
    public Image imgIconSkill4;

    [Header("3D ĳ���͸� �������� ī�޶�")]
    public Camera popupSelectCharacterCamera;
    [Header("�⺻ ī�޶�")]
    public Camera mainCamera;

    private CharacterCardController selectedCard;
    private CharacterCardController currentClickedCard;

    private void Awake()
    {
        btnBack.AddClickListener(Hide);
        btnSelectCharacter.AddClickListener(SelectCharacter);
        currentClickedCard = null;
    }

    public CharacterCardController GetCurrentClickedCard() 
    {
        return currentClickedCard;
    }

    public void UpdateCurrentClickedCard(CharacterCardController characterCard)
    {
        currentClickedCard  = characterCard;
        // ���� ī�� ���̶���Ʈ ȿ��
        UpdateCharacterCardsFocus();
        // ���� ĳ���� ���� ����
        ShowCharacterInfo(currentClickedCard.characterCardInfo);
        // ĳ���� ���� �ִϸ��̼�(�¸�����) ���
        lobbySceneUIController.PlayAnimCharacterVictory();
    }

    private void SelectCharacter()
    {
        if (currentClickedCard == null) {
            Hide();
            return;
        }

        //selectedCard?.SetFocus(false);
        selectedCard = currentClickedCard;
        //selectedCard.SetFocus(true);
        Hide();
    }

    private void ShowCharacterInfo(CharacterCardInfo characterCardInfo)
    {
        // ���õ� ĳ������ ������ ǥ��
        imgIconRole.sprite = characterCardInfo.iconRole;
        txtCharacterName.text = characterCardInfo.characterName;
        txtCharacterDescription.text = characterCardInfo.characterDescription;
        imgIconSkill1.sprite = GameAssetsManager.Instance.GetSpellIconImage(characterCardInfo.characterSkillSet[0]);
        imgIconSkill2.sprite = GameAssetsManager.Instance.GetSpellIconImage(characterCardInfo.characterSkillSet[1]);
        imgIconSkill3.sprite = GameAssetsManager.Instance.GetSpellIconImage(characterCardInfo.characterSkillSet[2]);
        imgIconSkill4.sprite = GameAssetsManager.Instance.GetSpellIconImage(characterCardInfo.characterSkillSet[3]);

        lobbySceneUIController.ChangePlayerCharacter(characterCardInfo.character);
    }

    public void Show()
    {
        gameObject.SetActive(true);
        SetupCharacterCards();  
        mainCamera.enabled = false;
        popupSelectCharacterCamera.enabled = true;
    }

    private void Hide()
    {
        gameObject.SetActive(false);
        lobbySceneUIController.ChangePlayerCharacter(selectedCard.characterCardInfo.character);
        mainCamera.enabled = true;
        popupSelectCharacterCamera.enabled = false;
    }

    private void SetupCharacterCards()
    {
        CharacterCardController[] formalData = containerCharacterCards.GetComponentsInChildren<CharacterCardController>();
        foreach(CharacterCardController characterCard in formalData)
        {
            Destroy(characterCard.gameObject);
        }

        foreach (CharacterCardInfo cardInfo in characterCardInfos)
        {
            GameObject characterCardObject = Instantiate(prefabCharacterCard, containerCharacterCards);
            characterCardObject.GetComponent<CharacterCardController>().InitCharacterCard(cardInfo);

            if(cardInfo.character == PlayerDataManager.Instance.GetCurrentPlayerClass())
            {
                selectedCard = characterCardObject.GetComponent<CharacterCardController>();
                selectedCard.SetFocus(true);
                ShowCharacterInfo(cardInfo);
            }
        }
    }

    private void UpdateCharacterCardsFocus()
    {
        CharacterCardController[] formalData = containerCharacterCards.GetComponentsInChildren<CharacterCardController>();
        foreach (CharacterCardController characterCard in formalData)
        {
            characterCard.SetFocus(false);
        }

        currentClickedCard.SetFocus(true);
    }
}

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

    // ȭ�鿡 ������ ĳ���� ī�� ������
    public CharacterCardInfo[] characterCardInfos;

    // ���õ� ĳ������ ������ ǥ���� UI ������Ʈ��
    public Image imgIconRole;
    public TextMeshProUGUI txtCharacterName;
    public TextMeshProUGUI txtCharacterDescription;
    public Image imgIconSkill1;
    public Image imgIconSkill2;
    public Image imgIconSkill3;
    public Image imgIconSkill4;


    // 3D ĳ���͸� �����ֱ� ���� ������
    public GameObject uiCanvas; // UI ĵ����
    public GameObject worldObject; // 3D ������Ʈ
    public Camera worldSpaceCamera; // 3D ������Ʈ�� �������� ī�޶�

    private CharacterCardController selectedCard;
    private CharacterCardController currentClickedCard;

    private void Awake()
    {
        btnBack.AddClickListener(Hide);
        btnSelectCharacter.AddClickListener(SelectCharacter);
        currentClickedCard = null;
    }

    public void UpdateCurrentClickedCard(CharacterCardController characterCard)
    {
        currentClickedCard  = characterCard;
        ShowCharacterInfo(currentClickedCard.characterCardInfo);
    }

    private void SelectCharacter()
    {
        if (currentClickedCard == null) return;
        selectedCard?.SetFocus(false);
        selectedCard = currentClickedCard;
        selectedCard.SetFocus(true);
    }

    private void ShowCharacterInfo(CharacterCardInfo characterCardInfo)
    {
        // ���õ� ĳ������ ������ ǥ��
        imgIconRole.sprite = characterCardInfo.iconRole;
        txtCharacterName.text = characterCardInfo.characterName;
        txtCharacterDescription.text = characterCardInfo.characterDescription;
        imgIconSkill1.sprite = GameAssets.instantiate.GetSpellIconImage(characterCardInfo.characterSkillSet[0]);
        imgIconSkill2.sprite = GameAssets.instantiate.GetSpellIconImage(characterCardInfo.characterSkillSet[1]);
        imgIconSkill3.sprite = GameAssets.instantiate.GetSpellIconImage(characterCardInfo.characterSkillSet[2]);
        imgIconSkill4.sprite = GameAssets.instantiate.GetSpellIconImage(characterCardInfo.characterSkillSet[3]);


        // 3D ������Ʈ�� ���� �����̽����� Screen Space - Camera�� ��ȯ
        lobbySceneUIController.ChangePlayerCharacter(characterCardInfo.character);
        worldObject = lobbySceneUIController.GetSelectedCharacter3DObject();
        Vector3 screenPos = worldSpaceCamera.WorldToScreenPoint(worldObject.transform.position);
        worldObject.transform.SetParent(uiCanvas.transform, true);
        worldObject.transform.position = screenPos;
    }

    public void Show()
    {
        gameObject.SetActive(true);
        SetupCharacterCards();  
    }

    private void Hide()
    {
        gameObject.SetActive(false);
        lobbySceneUIController.ChangePlayerCharacter(selectedCard.characterCardInfo.character);
    }

    private void SetupCharacterCards()
    {
        CharacterCardController[] formalData = containerCharacterCards.GetComponentsInChildren<CharacterCardController>();
        foreach(CharacterCardController characterCard in formalData)
        {
            Destroy(characterCard);
        }

        foreach (CharacterCardInfo cardInfo in characterCardInfos)
        {
            GameObject characterCardObject = Instantiate(prefabCharacterCard, containerCharacterCards);
            characterCardObject.GetComponent<CharacterCardController>().InitCharacterCard(cardInfo);

            if(cardInfo.character == PlayerDataManager.Instance.GetCurrentPlayerClass())
            {
                selectedCard = characterCardObject.GetComponent<CharacterCardController>();
                ShowCharacterInfo(cardInfo);
            }
        }
    }
}

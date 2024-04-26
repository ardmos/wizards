using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 1. Select(최종 선택) 버튼
/// 2. Select(최종 선택) 카드만 포커스 효과 실행
/// 3. 현재 클릭(미리보기 선택)된 카드 캐릭터 정보 중앙에 표시 ( 일단은 캐릭터 정보를 캐릭터 카드가 가지고 있다. )
/// 4. 돌아가기 버튼
/// </summary>
public class PopupSelectCharacterUIController : MonoBehaviour
{
    public LobbySceneUIController lobbySceneUIController;
    public CustomClickSoundButton btnBack;
    public CustomClickSoundButton btnSelectCharacter;

    public Transform containerCharacterCards;
    public GameObject prefabCharacterCard;

    // 화면에 보여줄 캐릭터 카드 정보들
    public CharacterCardInfo[] characterCardInfos;

    // 선택된 캐릭터의 정보를 표시할 UI 컴포넌트들
    public Image imgIconRole;
    public TextMeshProUGUI txtCharacterName;
    public TextMeshProUGUI txtCharacterDescription;
    public Image imgIconSkill1;
    public Image imgIconSkill2;
    public Image imgIconSkill3;
    public Image imgIconSkill4;


    // 3D 캐릭터를 렌더링할 카메라
    public Camera popupSelectCharacterCamera;
    // 기본 카메라
    public Camera mainCamera;

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
        // 캐릭터 선택 애니메이션(승리포즈) 재생
        lobbySceneUIController.PlayAnimCharacterVictory();
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
        // 선택된 캐릭터의 정보를 표시
        imgIconRole.sprite = characterCardInfo.iconRole;
        txtCharacterName.text = characterCardInfo.characterName;
        txtCharacterDescription.text = characterCardInfo.characterDescription;
        imgIconSkill1.sprite = GameAssets.instantiate.GetSpellIconImage(characterCardInfo.characterSkillSet[0]);
        imgIconSkill2.sprite = GameAssets.instantiate.GetSpellIconImage(characterCardInfo.characterSkillSet[1]);
        imgIconSkill3.sprite = GameAssets.instantiate.GetSpellIconImage(characterCardInfo.characterSkillSet[2]);
        imgIconSkill4.sprite = GameAssets.instantiate.GetSpellIconImage(characterCardInfo.characterSkillSet[3]);

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
}

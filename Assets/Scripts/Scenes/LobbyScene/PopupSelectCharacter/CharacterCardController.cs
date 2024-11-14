using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Character
//1.Role Icon
//2. Character Icon
//3. Name
//4. Description : 간단 정보(한 줄)
//5.Level :
//6.Skill Set
//7. IsSelected
//8. Character 3D 오브젝트 <-- 이건 Character로 GameAsset에서 받아올것임.
/// </summary>
public class CharacterCardController : MonoBehaviour
{
    // 캐릭터 카드 정보
    public CharacterCardInfo characterCardInfo;

    // 캐릭터 카드에서 표시할 UI 컴포넌트들
    public Image imgIconCharacter;
    public Image imgBG;
    public Image imgFocusBG;
    public TextMeshProUGUI txtCharacterLevel;

    // 캐릭터 카드 클릭 인식용 버튼
    public CustomClickSoundButton button;

    private void Awake()
    {
        button.AddClickListener(() => {
            FindObjectOfType<PopupSelectCharacterUIController>().UpdateCurrentClickedCard(this);
        });
    }

    public void SetFocus(bool isFocusOn)
    {
        imgFocusBG.enabled = isFocusOn;
    } 

    public void InitCharacterCard(CharacterCardInfo characterCardInfo)
    {
        this.characterCardInfo = characterCardInfo;

        imgIconCharacter.sprite = this.characterCardInfo.iconCharacter;
        imgBG.sprite = this.characterCardInfo.spriteBG;
        txtCharacterLevel.text = this.characterCardInfo.characterLevel.ToString();
        SetFocus(false);
    }


}

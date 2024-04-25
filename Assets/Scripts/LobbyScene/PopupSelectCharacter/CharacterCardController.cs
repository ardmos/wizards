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
//4. Description : ���� ����(�� ��)
//5.Level :
//6.Skill Set
//7. IsSelected
//8. Character 3D ������Ʈ <-- �̰� Character�� GameAsset���� �޾ƿð���.
/// </summary>
public class CharacterCardController : MonoBehaviour
{
    // ĳ���� ī�� ����
    public CharacterCardInfo characterCardInfo;

    // ĳ���� ī�忡�� ǥ���� UI ������Ʈ��
    public Image imgIconCharacter;
    public Image imgBG;
    public Image imgFocusBG;
    public TextMeshProUGUI txtCharacterLevel;

    // ĳ���� ī�� Ŭ�� �νĿ� ��ư
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

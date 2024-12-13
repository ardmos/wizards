using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillGroupItemController : MonoBehaviour
{
    public CustomClickSoundButton btnClick;
    public int skillIndex;
    public MiniPopupSkillInfoDetailController miniPopupSkillInfoDetail;
    public PopupSelectCharacterUIController popupSelectCharacterUIController;

    private void Start()
    {
        btnClick.AddClickListener(OnClick);
    }

    private void OnClick()
    {
        miniPopupSkillInfoDetail.Show();
        miniPopupSkillInfoDetail.SetData(popupSelectCharacterUIController.GetCurrentClickedCard().characterCardInfo.skillInfoDetails[skillIndex]);
    }

}

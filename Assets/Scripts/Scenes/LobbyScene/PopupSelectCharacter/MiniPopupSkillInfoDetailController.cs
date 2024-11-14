using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MiniPopupSkillInfoDetailController : MonoBehaviour
{
    public CustomClickSoundButton btnClose;
    public CustomClickSoundButton btnScreenDim;

    public Image iconSkill;
    public TextMeshProUGUI txtSkillName;
    public TextMeshProUGUI txtSkillDescription;

    private void Start()
    {
        btnClose.AddClickListener(Hide);
        btnScreenDim.AddClickListener(Hide);
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void SetData(SkillInfoDetail skillInfoDetail)
    {
        iconSkill.sprite = skillInfoDetail.skillIcon;
        txtSkillName.text = skillInfoDetail.skillName;
        txtSkillDescription.text = skillInfoDetail.skillDescription;
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}

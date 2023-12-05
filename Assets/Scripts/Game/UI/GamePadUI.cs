using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 1. 조작 버튼에 현재 스킬 이미지 띄워주기
/// </summary>
public class GamePadUI : MonoBehaviour
{
    public TextMeshProUGUI txtButtonWest, txtButtonNorth, txtButtonEast;
    public Image imgCooltimeWest, imgCooltimeNorth, imgCooltimeEast;
    public GameManager gameManager;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        UpdateButtonText();
        CoolTimePresenter();
    }

    private void UpdateButtonText()
    {
        if (Player.LocalInstance == null)
        {
            return;
        }

        txtButtonWest.text = Player.LocalInstance.gameObject.GetComponent<SpellController>().GetCurrentSpell1Name();
        txtButtonNorth.text = Player.LocalInstance.gameObject.GetComponent<SpellController>().GetCurrentSpell2Name();
        txtButtonEast.text = Player.LocalInstance.gameObject.GetComponent<SpellController>().GetCurrentSpell3Name();    
    }

    private void CoolTimePresenter()
    {
        if (Player.LocalInstance == null)
        {
            return;
        }

        float spell1CoolTimeRatio = Player.LocalInstance.gameObject.GetComponent<SpellController>().GetCurrentSpell1CoolTimeRatio();
        if (spell1CoolTimeRatio > 0) {
            imgCooltimeWest.fillAmount = 1 - spell1CoolTimeRatio;
        }
        float spell2CoolTimeRatio = Player.LocalInstance.gameObject.GetComponent<SpellController>().GetCurrentSpell2CoolTimeRatio();
        if (spell2CoolTimeRatio > 0)
        {
            imgCooltimeNorth.fillAmount = 1 - spell2CoolTimeRatio;
        }
        float spell3CoolTimeRatio = Player.LocalInstance.gameObject.GetComponent<SpellController>().GetCurrentSpell3CoolTimeRatio();
        if (spell3CoolTimeRatio > 0)
        {
            imgCooltimeEast.fillAmount = 1 - spell3CoolTimeRatio;
        }
    }
}

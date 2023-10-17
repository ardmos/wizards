using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
/// <summary>
/// 1. 조작 버튼에 현재 스킬 이름 띄워주기
/// </summary>
public class UI_GamePad : MonoBehaviour
{
    public TextMeshProUGUI txtButtonWest, txtButtonNorth, txtButtonEast;
    public GameManager gameManager;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        UpdateButtonText();
    }

    public void UpdateButtonText()
    {
        if (gameManager.ownerPlayerObject != null)
        {
            txtButtonWest.text = gameManager.ownerPlayerObject.GetComponent<SpellController>().GetCurrentSpell1Name();
            txtButtonNorth.text = gameManager.ownerPlayerObject.GetComponent<SpellController>().GetCurrentSpell2Name();
            txtButtonEast.text = gameManager.ownerPlayerObject.GetComponent<SpellController>().GetCurrentSpell3Name();
        }
        
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.UI;
/// <summary>
/// 1. 조작 버튼에 현재 스킬 이미지 띄워주기
/// 2. 쿨타임 시각효과로 보여주기
/// </summary>
public class GamePadUIController : MonoBehaviour
{
    public Image[] spellIcons;
    public Image[] spellCooltimeImages;
    
    // Update is called once per frame
    void Update()
    {
        CoolTimePresenter();
    }

    /// <summary>
    /// 게임패드 스킬 아이콘을 업데이트 해주는 메소드
    /// </summary>
    public void UpdateSpellUI(SpellName[] spellNames)
    {
        if (Player.Instance == null) return;

        for (ushort i = 0; i < spellNames.Length; i++)
        {
            //Debug.Log($"spellName: {spellNames[i]}");
            spellIcons[i].sprite = GameAssets.instantiate.GetSpellIconImage(spellNames[i]);
        }       
    }

    /// <summary>
    /// 쿨타임을 시각화해주는 메소드
    /// </summary>
    private void CoolTimePresenter()
    {
        if (Player.Instance == null)
        {
            return;
        }

        for (byte i = 0;i < spellCooltimeImages.Length; i++)
        {
            //Debug.Log($"{Player.LocalInstance.OwnerClientId}");
            float coolTimeRatio = Player.Instance.GetComponent<SpellController>().GetCurrentSpellCoolTimeRatio(i);
            if (coolTimeRatio > 0) 
            {
                spellCooltimeImages[i].fillAmount = 1 - coolTimeRatio;
            }
        }
    }
}

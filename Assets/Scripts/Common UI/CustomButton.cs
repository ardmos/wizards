using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// UI 버튼 클릭 사운드 추가를 위한 커스텀 버튼 스크립트 입니다.
/// </summary>
public class CustomButton : Button
{
    public void AddClickListener(UnityAction call)
    {
        onClick.AddListener(()=> {
            SoundManager.instance.PlayButtonClickSound();
            call(); 
        });
    }
}

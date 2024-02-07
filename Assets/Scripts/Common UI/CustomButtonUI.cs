using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// UI ��ư Ŭ�� ���� �߰��� ���� Ŀ���� ��ư ��ũ��Ʈ �Դϴ�.
/// </summary>
public class CustomButtonUI : Button
{
    public void AddClickListener(UnityAction call)
    {
        onClick.AddListener(()=> {
            SoundManager.instance.PlayButtonClickSound();
            call(); 
        });
    }
}

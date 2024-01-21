using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Game씬 결과 팝업중 Win 팝업에 등장하는 Item의 템플릿 컨트롤러 스크립트 입니다.
/// 하는 일
/// 1. 아이템 아이콘 이미지를 정합니다
/// 2. 아이템 개수 표시를 합니다
/// 3. 보너스 등 상단 추가 표시 여부를 결정합니다
/// 
/// </summary>
public class WinPopupItemTemplate : MonoBehaviour
{
    [SerializeField] private Image imgItemIcon;
    [SerializeField] private TextMeshProUGUI txtItemCount;
    [SerializeField] private GameObject label;

    public void InitTemplate(Sprite imgItemIcon, string txtItemCount, bool isLabelOn)
    {
        this.imgItemIcon.sprite = imgItemIcon;
        this.imgItemIcon.SetNativeSize();
        this.txtItemCount.text = txtItemCount;
        if (label != null ) this.label.SetActive(isLabelOn);
    }   
}

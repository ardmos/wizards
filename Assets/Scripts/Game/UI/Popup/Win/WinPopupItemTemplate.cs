using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Game�� ��� �˾��� Win �˾��� �����ϴ� Item�� ���ø� ��Ʈ�ѷ� ��ũ��Ʈ �Դϴ�.
/// �ϴ� ��
/// 1. ������ ������ �̹����� ���մϴ�
/// 2. ������ ���� ǥ�ø� �մϴ�
/// 3. ���ʽ� �� ��� �߰� ǥ�� ���θ� �����մϴ�
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

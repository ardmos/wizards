using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Ÿ��Ʋ���� UI�� ��Ʈ���ϴ� ��ũ��Ʈ �Դϴ�
/// !!���� ���
///     1. ���� ���� ��ư(BtnStartAuth) Ŭ���� ����������� �˾�(PopupSelectAuthMethod) ����
/// </summary>
public class TitleSceneUI : MonoBehaviour
{
    [SerializeField] private PopupSelectAuthMethodUI popupSelectAuthMethod;
    [SerializeField] private Button btnStartAuth;

    // Start is called before the first frame update
    void Start()
    {
        btnStartAuth.onClick.AddListener(OnBtnStartAuthClick);
    }

    public void OnBtnStartAuthClick() {
        SoundManager.instance.PlayButtonClickSound();
        popupSelectAuthMethod.Show();
    }
}

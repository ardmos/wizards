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
    [SerializeField] private GameObject popupSelectAuthMethod;
    [SerializeField] private Button btnStartAuth;

    // Start is called before the first frame update
    void Start()
    {
        // ���ο� UGS ��Ʈ��ũ��� �׽�Ʈ�ϴ���. ��ŸƮ��ư�� ����� ������ �����ϴµ� ���Ŷ� ��� �ּ�ó��
        //btnStartAuth.onClick.AddListener(OnBtnStartAuthClick);
    }

    public void OnBtnStartAuthClick() {
        popupSelectAuthMethod.transform.localScale = Vector3.one;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Ÿ��Ʋ���� ���� ���� ��ư(BtnStartAuth)�� ��Ʈ���ϴ� ��ũ��Ʈ �Դϴ�
/// !!���� ���
///     1. Ŭ���� ����������� �˾�(PopupSelectAuthMethod) ����
/// </summary>
public class BtnStartAuthController : MonoBehaviour
{
    [SerializeField]
    private GameObject popupSelectAuthMethod;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnBtnStartAuthClick() {
        popupSelectAuthMethod.transform.localScale = Vector3.one;
    }
}

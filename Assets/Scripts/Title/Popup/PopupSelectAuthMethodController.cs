using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Ÿ��Ʋ�� �α��� ���� ��� ���� �˾�(PopupSelectAuthMethod)�� ��Ʈ���ϴ� ��ũ��Ʈ �Դϴ�.
/// !!���� ���
///     1. �ܰ� ��ο� ����(BtnCloseBackShadow) Ŭ���� �˾� �ݱ�
/// </summary>

public class PopupSelectAuthMethodController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnBtnCloseClicked()
    {
        gameObject.transform.localScale = Vector3.zero;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 타이틀씬의 인증 시작 버튼(BtnStartAuth)을 컨트롤하는 스크립트 입니다
/// !!현재 기능
///     1. 클릭시 인증방법선택 팝업(PopupSelectAuthMethod) 열기
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

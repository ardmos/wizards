using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 타이틀씬의 UI를 컨트롤하는 스크립트 입니다
/// !!현재 기능
///     1. 인증 시작 버튼(BtnStartAuth) 클릭시 인증방법선택 팝업(PopupSelectAuthMethod) 열기
/// </summary>
public class TitleSceneUI : MonoBehaviour
{
    [SerializeField] private GameObject popupSelectAuthMethod;
    [SerializeField] private Button btnStartAuth;

    // Start is called before the first frame update
    void Start()
    {
        // 새로운 UGS 네트워크방식 테스트하는중. 스타트버튼의 기능을 서버에 접속하는데 쓸거라서 잠시 주석처리
        //btnStartAuth.onClick.AddListener(OnBtnStartAuthClick);
    }

    public void OnBtnStartAuthClick() {
        popupSelectAuthMethod.transform.localScale = Vector3.one;
    }
}

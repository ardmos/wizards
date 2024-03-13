using UnityEngine;
/// <summary>
/// 타이틀씬의 UI를 컨트롤하는 스크립트 입니다
/// !!현재 기능
///     1. 인증 시작 버튼(BtnStartAuth) 클릭시 인증방법선택 팝업(PopupSelectAuthMethod) 열기
/// </summary>
public class TitleSceneUIController : MonoBehaviour
{
    [SerializeField] private PopupSelectAuthMethodUIController popupSelectAuthMethod;
    [SerializeField] private CustomClickSoundButton btnStartAuth;

    // Start is called before the first frame update
    void Start()
    {
        btnStartAuth.AddClickListener(OnBtnStartAuthClick);
    }

    public void OnBtnStartAuthClick() {
        popupSelectAuthMethod.Show();
    }
}

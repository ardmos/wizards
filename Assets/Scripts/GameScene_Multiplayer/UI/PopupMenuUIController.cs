using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PopupMenuUIController : MonoBehaviour
{
    // 메뉴팝업 닫기
    [SerializeField] private CustomClickSoundButton btnContinue;
    // 로비로
    [SerializeField] private CustomClickSoundButton btnHome;
    // 게임 포기. 관전하기 ( 관전시스템 추후 구축 )

    private void Start()
    {
        btnContinue.AddClickListener(Hide);
        btnHome.AddClickListener(() =>
        {
            // 로비씬으로 이동
            LoadSceneManager.Load(LoadSceneManager.Scene.LobbyScene);
        });
        Hide();
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}

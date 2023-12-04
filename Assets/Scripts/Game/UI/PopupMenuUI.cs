using UnityEngine;
using UnityEngine.UI;

public class PopupMenuUI : MonoBehaviour
{
    // 메뉴팝업 닫기
    [SerializeField] private Button btnClose;
    // 로비로
    [SerializeField] private Button btnRestart;
    // 게임 포기. 관전하기 ( 관전시스템 추후 구축 )


    // Start is called before the first frame update
    void Start()
    {
        btnClose.onClick.AddListener(Hide);
        btnRestart.onClick.AddListener(() =>
        {
            // 이동 전에 UGS Multiplay 플레이 종료 처리 이곳에 추가하면 됨

            // 로비씬으로 이동
            LoadingSceneManager.Load(LoadingSceneManager.Scene.LobbyScene);
        });
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

using UnityEngine;
/// <summary>
/// Ÿ��Ʋ���� UI�� ��Ʈ���ϴ� ��ũ��Ʈ �Դϴ�
/// !!���� ���
///     1. ���� ���� ��ư(BtnStartAuth) Ŭ���� ����������� �˾�(PopupSelectAuthMethod) ����
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

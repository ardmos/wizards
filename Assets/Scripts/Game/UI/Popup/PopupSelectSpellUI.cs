using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
/// <summary>
/// ��ũ�� ȹ��� ȹ���� ��ũ���� ��� ���翡 �������� 
/// �����ϴ� �˾��� �����ϴ� ��ũ��Ʈ �Դϴ�.
/// 
/// 1. � ��ų �����ؼ� �� �˾��� �����ԵȰ��� ��ũ�� ������ ���� ����
/// 1. ��ư �� �߿� ���� -> ���õ� ������ ��ų�� ��ũ�� ������ ���� ����.
/// </summary>
public class PopupSelectSpellUI : MonoBehaviour
{
    // �� �˾��� �����Ե� ��ũ�� ������ ���� ����
    [SerializeField] private Scroll scroll;
    
    public Button btnSpell1;
    public Button btnSpell2;
    public Button btnSpell3;



    private void Start()
    {
        btnSpell1.onClick.AddListener(() => {
            Player.LocalInstance.ApplyScrollToSpell(scroll, 0);
        });
        btnSpell2.onClick.AddListener(() => {
            Player.LocalInstance.ApplyScrollToSpell(scroll, 1);
        });
        btnSpell3.onClick.AddListener(() => {
            Player.LocalInstance.ApplyScrollToSpell(scroll, 2);
        });

        Hide();
    }


    public void Show(Scroll scroll)
    {
        gameObject.SetActive(true);
        this.scroll = scroll;
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
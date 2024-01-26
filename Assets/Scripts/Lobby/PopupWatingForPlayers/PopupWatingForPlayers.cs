using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ���ӷ���� UI�� ��ü�ϱ�� �߽��ϴ�.
/// ���ӷ���� ����ϴ� UI�Դϴ�. 
/// Ŭ���̾�Ʈ ���忡�� �����ϴ� UI �Դϴ�.
///
/// 1. ��Ī �ο� ����� ���׶��
/// 2. ��Ī �ο� �߰��� ���׶�� üũ ǥ�� 
/// 3. ��Ī �ο� ����� �ش� üũǥ�� ����
/// 4. ��Ī �ο� �� á�� �� ���Ӿ����� �̵�
/// 5. ��Ī ��� ��ư �߰�. 
/// 6. ��Ī ��� ��ư Ŭ���� �ش� ���� ��Ī ���.
/// </summary>
public class PopupWatingForPlayers : MonoBehaviour
{
    public Button btnCancel;

    // Start is called before the first frame update
    void Start()
    {
        GameMultiplayer.Instance.OnSucceededToJoinMatch += OnSucceededToJoinMatch;
        GameMultiplayer.Instance.OnFailedToJoinMatch += OnFailedToJoinMatch;

        btnCancel.onClick.AddListener(CancelMatch);

        Hide();
    }

    /// <summary>
    /// ���� �÷��̾ ��Ī Ƽ�� ������ �����ϸ� ȣ��� �޼ҵ� �Դϴ�. 
    /// �� UI�� �ݾ��ݴϴ�. ���� ���� ����� PopupConnectionResponseUI���� ���ݴϴ�.
    /// </summary>
    private void OnFailedToJoinMatch(object sender, System.EventArgs e)
    {
        Hide();
    }

    /// <summary>
    /// ���� �÷��̾ ��Ī Ƽ�� ������ �����ϸ� ȣ��Ǵ� �޼ҵ� �Դϴ�.
    /// �� UI�� �����ݴϴ�.
    /// </summary>
    private void OnSucceededToJoinMatch(object sender, System.EventArgs e)
    {
        Show();
        
        // �Ʒ� �� �÷��̾���ڸ� �� �� �ʶ��ϰ� �޾ƿͺ���. ������� GameMultiplayer���� �Լ��� ��û�Ѵٴ���./ <<< �������!
        Debug.Log($"(Ŭ���̾�Ʈ)���� �������� �� �÷��̾� �� : {GameMultiplayer.Instance.GetPlayerDataNetworkList().Count}");
    }

    /// <summary>
    /// ���� ��Ī���� Ƽ�Ͽ��� �����ϴ�. 
    /// </summary>
    private void CancelMatch()
    {
        GameMultiplayer.Instance.StopClient();
        Hide();
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}

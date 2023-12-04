using TMPro;
using UnityEngine;

public class PlayerCountUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI txtAlivePlayerCount;

    // Start is called before the first frame update
    void Start()
    {
        GameManager.Instance.OnAlivePlayerCountChanged += GameManager_OnAlivePlayerCountChanged;
    }

    private void GameManager_OnAlivePlayerCountChanged(object sender, System.EventArgs e)
    {
        txtAlivePlayerCount.text = GameManager.Instance.GetCurrentAlivePlayerCount().ToString();
    }
}

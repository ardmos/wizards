using UnityEngine;
using UnityEngine.UI;

public class PopupNewsUIController : MonoBehaviour
{
    [SerializeField] private Button btnClose;

    // Start is called before the first frame update
    void Start()
    {
        btnClose.onClick.AddListener(()=> { SoundManager.Instance.PlayButtonClickSound(); Hide(); });
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

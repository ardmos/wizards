using UnityEngine;
using UnityEngine.UI;

public class PopupNewsUI : MonoBehaviour
{
    [SerializeField] private Button btnClose;

    // Start is called before the first frame update
    void Start()
    {
        btnClose.onClick.AddListener(Hide);
    }

    // Update is called once per frame
    void Update()
    {

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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupClanUIController : MonoBehaviour
{
    [SerializeField] private Button btnClose;
    [SerializeField] private Button btnHome;

    // Start is called before the first frame update
    void Start()
    {
        btnClose.onClick.AddListener(() => { SoundManager.Instance.PlayButtonClickSound(); Hide(); });
        btnHome.onClick.AddListener(() => { SoundManager.Instance.PlayButtonClickSound(); Hide(); });
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

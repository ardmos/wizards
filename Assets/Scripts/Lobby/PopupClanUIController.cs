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
        btnClose.onClick.AddListener(() => { SoundManager.instance.PlayButtonClickSound(); Hide(); });
        btnHome.onClick.AddListener(() => { SoundManager.instance.PlayButtonClickSound(); Hide(); });
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

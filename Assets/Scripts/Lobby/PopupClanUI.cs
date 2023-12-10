using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupClanUI : MonoBehaviour
{
    [SerializeField] private Button btnClose;
    [SerializeField] private Button btnHome;

    // Start is called before the first frame update
    void Start()
    {
        btnClose.onClick.AddListener(Hide);
        btnHome.onClick.AddListener(Hide);
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

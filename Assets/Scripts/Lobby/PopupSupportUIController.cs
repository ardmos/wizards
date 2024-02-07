using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupSupportUIController : MonoBehaviour
{
    public CustomButtonUI btnClose;

    // Start is called before the first frame update
    void Start()
    {
        btnClose.AddClickListener(Hide);
        Hide();
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

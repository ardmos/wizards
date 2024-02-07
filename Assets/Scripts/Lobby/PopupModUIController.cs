using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupModUIController : MonoBehaviour
{
    public CustomButtonUI btnClose;
    public CustomButtonUI btnHome;

    // Start is called before the first frame update
    void Start()
    {
        btnClose.AddClickListener(Hide);
        btnHome.AddClickListener(Hide);
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

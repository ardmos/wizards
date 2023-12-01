using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupShopUI : MonoBehaviour
{
    [SerializeField] private Button btnBack;
    [SerializeField] private Button btnHome;

    // Start is called before the first frame update
    void Start()
    {
        btnBack.onClick.AddListener(Hide);
        btnHome.onClick.AddListener(Hide);

        //Hide();
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

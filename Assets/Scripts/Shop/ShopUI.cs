using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    [SerializeField] private Button btnBack;
    [SerializeField] private Button btnLobby;


    // Start is called before the first frame update
    void Start()
    {
        btnBack.onClick.AddListener(() => {

            });
        btnLobby.onClick.AddListener(() => {
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }


}

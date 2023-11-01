using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectUI : MonoBehaviour
{
    [SerializeField] private Button btnReady;

    private void Awake()
    {
        btnReady.onClick.AddListener(()=>
        {
            CharacterSelectReady.Instance.SetPlayerReady();
        });
    }
}

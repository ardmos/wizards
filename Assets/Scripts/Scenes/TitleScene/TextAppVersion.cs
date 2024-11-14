using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextAppVersion : MonoBehaviour
{
    public TextMeshProUGUI textMeshProUGUI;

    public void Start()
    {
        textMeshProUGUI.text = "ver " + Application.version;
    }
}

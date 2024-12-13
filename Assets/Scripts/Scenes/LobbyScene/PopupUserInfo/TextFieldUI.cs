using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// InputField ga anin changable text with placeHolder.  Controll script 
/// </summary>
public class TextFieldUI : MonoBehaviour
{
    public TextMeshProUGUI txtPlaceHolder;
    public TextMeshProUGUI txtTextField;

    // Start is called before the first frame update
    void Start()
    {
        txtPlaceHolder.enabled = (txtTextField.text.Length == 0);
    }

    public void UpdatePlayerHolderValue(string newText)
    {
        txtPlaceHolder.text = newText;
    }

    public void UpdateTextValue(string newText)
    {
        txtPlaceHolder.enabled = (newText.Length == 0);
                      
        txtTextField.text = newText;
    }
}

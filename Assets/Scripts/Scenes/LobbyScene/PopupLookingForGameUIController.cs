using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PopupLookingForGameUIController : MonoBehaviour
{
    public TextMeshProUGUI textMeshProUGUI;

    private WaitForSeconds waitForSeconds = new WaitForSeconds(1);
    [SerializeField] private string[] texts;

    public void Show()
    { 
        gameObject.SetActive(true);
        StartCoroutine(TextAnimation());
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        StopCoroutine(TextAnimation());
    }

    private IEnumerator TextAnimation()
    {
        while (true)
        {
            for(int i = 0; i < texts.Length; i++)
            {
                textMeshProUGUI.text = texts[i];
                yield return waitForSeconds;
            }
        }
    }
}

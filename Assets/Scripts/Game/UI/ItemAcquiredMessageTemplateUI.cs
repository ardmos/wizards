using TMPro;
using UnityEngine;

public class ItemAcquiredMessageTemplateUI : MonoBehaviour
{
    public TextMeshProUGUI txtMessage;

    void Start()
    {
        txtMessage.text = "Obtained the scroll!";
        Destroy(gameObject, 3f);
    }
}

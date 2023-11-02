using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HPBarUI : MonoBehaviour
{
    public Image hpFill;

    public void SetHP(float value)
    {
        hpFill.fillAmount = value;
    }
}

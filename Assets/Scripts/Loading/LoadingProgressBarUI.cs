using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 비동기 로딩의 진행상황을 화면에 보여주기 위한 스크립트. 
/// 현재는 텍스트롤 보여준다
/// </summary>
public class LoadingProgressBarUI : MonoBehaviour
{
    public Image imgFill;

    private void Awake()
    {
        imgFill.fillAmount = 0;
    }

    // Update is called once per frame
    void Update()
    {
        imgFill.fillAmount = LoadingSceneManager.GetLoadingProgress();
    }
}

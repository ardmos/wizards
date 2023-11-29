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
    [SerializeField] private Slider progressBar;
    [SerializeField] private TextMeshProUGUI txtValue;
    [SerializeField] private int progressValue;

    private void Awake()
    {
        progressBar.value  = 0f;
        UpdateTxtValue();
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("Loading Progess : " + LoadingSceneManager.GetLoadingProgress());
        //progressBar.value = LoadingSceneManager.GetLoadingProgress() * 100f;
        UpdateTxtValue();
    }

    private void UpdateTxtValue()
    {
        progressValue = (int)progressBar.value;
        txtValue.text = $"Loading... {progressValue}%";
    }
}

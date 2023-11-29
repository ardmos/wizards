using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// �񵿱� �ε��� �����Ȳ�� ȭ�鿡 �����ֱ� ���� ��ũ��Ʈ. 
/// ����� �ؽ�Ʈ�� �����ش�
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

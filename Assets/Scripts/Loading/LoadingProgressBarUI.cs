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

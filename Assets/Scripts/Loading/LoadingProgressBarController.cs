using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// �񵿱� �ε��� �����Ȳ�� ȭ�鿡 �����ֱ� ���� ��ũ��Ʈ. 
/// ����� �ؽ�Ʈ�� �����ش�
/// </summary>
public class LoadingProgressBarController : MonoBehaviour
{
    public Image imgFill;
    
    // Update is called once per frame
    void Update()
    {
        imgFill.fillAmount = LoadingSceneManager.GetLoadingProgress();
    }
}

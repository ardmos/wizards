using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// �ε����� ȭ���� ����ֱ� ���� ��ũ��Ʈ. 
/// ȭ���� ������� ���� LoaddingSceneManager.LoaderCallback() �޼��带 ���ؼ� Ÿ�� ���� �񵿱� �ε��� ���۵ȴ�.
/// </summary>
public class LoaderCallback : MonoBehaviour
{
    private bool isFirstUpdate = true;

    // Update is called once per frame
    void Update()
    {
        if (isFirstUpdate)
        {
            isFirstUpdate = false;
            LoadSceneManager.LoaderCallback();
        }
    }
}

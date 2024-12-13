using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 로딩씬의 화면을 띄워주기 위한 스크립트. 
/// 화면이 띄워지고 나면 LoaddingSceneManager.LoaderCallback() 메서드를 통해서 타겟 씬의 비동기 로딩이 시작된다.
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

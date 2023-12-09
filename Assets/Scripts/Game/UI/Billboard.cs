using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// UI가 화면을 정면으로 쳐다보게 해주는 스크립트
/// </summary>
public class Billboard : MonoBehaviour
{
    private Transform mCamera;

    private void Start()
    {
        mCamera = Camera.main.transform;
    }

    private void LateUpdate()
    {
        transform.LookAt(transform.position + mCamera.forward);
    }
}

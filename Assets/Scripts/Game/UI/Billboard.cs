using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// UI�� ȭ���� �������� �Ĵٺ��� ���ִ� ��ũ��Ʈ
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

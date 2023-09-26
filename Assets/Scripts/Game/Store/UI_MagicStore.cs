using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// ���������� UI�� �����ϴ� ��ũ��Ʈ.
/// !!! ���� ���
///     1. UIȰ��ȭ/��Ȱ��ȭ
/// </summary>
public class UI_MagicStore : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // �⺻ ��Ȱ��ȭ ����.
        // ��Ȱ��ȭ ������ ���� ǰ�� �����ʹ� �ε尡 ��������
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    /// <summary>
    /// UI ���������� Ȱ��ȭ ��Ű�� �޼��� 
    /// </summary>
    public void Show(IStoreCustomer storeCustomer)
    {
        Debug.Log("ON");
        gameObject.SetActive(true);
    }
    /// <summary>
    /// UI ���������� ��Ȱ��ȭ ��Ű�� �޼���
    /// </summary>
    public void Hide()
    {
        Debug.Log("OFF");
        gameObject.SetActive(false);
    }
}

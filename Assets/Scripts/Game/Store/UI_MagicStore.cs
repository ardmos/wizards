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
        gameObject.SetActive(true);
    }
    /// <summary>
    /// UI ���������� ��Ȱ��ȭ ��Ű�� �޼���
    /// </summary>
    public void Hide()
    {
        gameObject.SetActive(false);
    }
}

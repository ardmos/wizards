using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// �÷��̾��� ���� ���� ���θ� �����ؼ� ������ Ȱ��/��Ȱ�� ��Ű�� ��ũ��Ʈ
/// !!!���� ���
///     1. �÷��̾� ���ٽ� �մ����� ���
///     2. �÷��̾� ���ٽ� ���� �˾� Ȱ��ȭ
///     3. �÷��̾� ��Ż�� ���� �˾� ��Ȱ��ȭ
/// </summary>
public class StoreTriggerAreaCollider : MonoBehaviour
{
    [SerializeField] private MagicStoreUI uiMagicStore;

    private void OnTriggerEnter(Collider other)
    {
        IStoreCustomer storeCustomer = other.GetComponent<IStoreCustomer>();
        //Debug.Log("IN"+ storeCustomer + ", who: " + other.name);
        if (storeCustomer != null)
        {
            uiMagicStore.Show(storeCustomer);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        //Debug.Log("OUT");
        IStoreCustomer storeCustomer = other.GetComponent<IStoreCustomer>();
        if (storeCustomer != null)
        {
            uiMagicStore.Hide();
        }
    }
}

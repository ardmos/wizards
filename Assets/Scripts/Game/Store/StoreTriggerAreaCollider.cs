using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 플레이어의 상점 접근 여부를 감지해서 상점을 활성/비활성 시키는 스크립트
/// !!!현재 기능
///     1. 플레이어 접근시 손님으로 등록
///     2. 플레이어 접근시 상점 팝업 활성화
///     3. 플레이어 이탈시 상점 팝업 비활성화
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

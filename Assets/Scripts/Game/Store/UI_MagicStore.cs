using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 매직상점의 UI를 관리하는 스크립트.
/// !!! 현재 기능
///     1. UI활성화/비활성화
/// </summary>
public class UI_MagicStore : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // 기본 비활성화 상태.
        // 비활성화 이전에 상점 품목 데이터는 로드가 끝나야함
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    /// <summary>
    /// UI 매직상점을 활성화 시키는 메서드 
    /// </summary>
    public void Show(IStoreCustomer storeCustomer)
    {
        Debug.Log("ON");
        gameObject.SetActive(true);
    }
    /// <summary>
    /// UI 매직상점을 비활성화 시키는 메서드
    /// </summary>
    public void Hide()
    {
        Debug.Log("OFF");
        gameObject.SetActive(false);
    }
}

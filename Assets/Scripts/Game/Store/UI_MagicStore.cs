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
        gameObject.SetActive(true);
    }
    /// <summary>
    /// UI 매직상점을 비활성화 시키는 메서드
    /// </summary>
    public void Hide()
    {
        gameObject.SetActive(false);
    }
}

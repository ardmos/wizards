using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageTextUIController : MonoBehaviour
{
    // ������ �ؽ�Ʈ ������Ʈ ����
    public void CreateTextObject(sbyte damageAmount)
    {
        GameObject damageTextObj = Instantiate(GameAssetsManager.Instance.gameAssets.vfx_txtDamageValue, transform.position, Quaternion.identity);
        damageTextObj.transform.SetParent(transform, false);
        damageTextObj.transform.localPosition = Vector3.zero;
        DamageTextUI damageTextUI = damageTextObj.GetComponent<DamageTextUI>();
        damageTextUI.Setup(damageAmount);       
    } 
}

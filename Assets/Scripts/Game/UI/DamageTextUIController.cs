using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageTextUIController : MonoBehaviour
{
    // ������ �ؽ�Ʈ ������Ʈ ����
    public void CreateTextObject(byte damageAmount)
    {
        GameObject damageTextObj = Instantiate(GameAssets.instantiate.txtDamageValue, transform.position, Quaternion.identity);
        damageTextObj.transform.SetParent(transform, false);
        damageTextObj.transform.localPosition = Vector3.zero;
        DamageTextUI damageTextUI = damageTextObj.GetComponent<DamageTextUI>();
        damageTextUI.Setup(damageAmount);       
    } 
}

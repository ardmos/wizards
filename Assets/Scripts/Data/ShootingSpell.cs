using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �߻� ������ �������� �Ӽ����� ���� Ŭ����
/// </summary>
public abstract class ShootingSpell : MonoBehaviour
{
    public SpellData data;
    bool castBtnClicked = false;
    bool castAble = true;
    float restTime = 0;

    public abstract void InitSpellDetail();

    public virtual void CastSpell(Transform muzzle)
    {
        if (castBtnClicked && castAble)  ///  castBtnClicked ��ſ� InputSystem ����ؾ���.  ���ݺ��� ��� InputSystem ����� ����
        {
            GameObject spellObject = Instantiate(data.spellObject);
            spellObject.transform.position = muzzle.position;

            castAble = false;
        }
        if (castAble == false)
        {
            restTime += Time.deltaTime;
            if (restTime >= data.coolTime)
            {
                castAble = true;
                restTime = 0f;
            }
        }
    }
}

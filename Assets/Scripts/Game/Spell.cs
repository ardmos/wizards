using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// ������ �������� �Ӽ����� ���� Ŭ����
/// </summary>
public abstract class Spell : MonoBehaviour
{
    public SpellData data;
    bool castAble = true;
    float restTime = 0;

    /// <summary>
    /// �� ������ �����ϰ��� ���� ���ε��� ��ӹ޾Ƽ� ����
    /// </summary>
    public abstract void InitSpellDetail();

    /// <summary>
    /// �� �÷��̾��� ���� ĳ��Ʈ ��Ʈ�ѷ��� Update �޼ҵ忡�� �Է�ó�� ����� ���� ȣ��� �޼ҵ�.
    /// </summary>
    /// <param name="muzzle"> ������ �߻�� ��ġ </param>
    public virtual void CastSpell(Transform muzzle)
    {
        if (castAble)  // inputSystem�� ���� �Է� üũ�� MagicSpellCastController���� �մϴ�. ��ų ��ư�� �����̱� ����.<--- ������� �ϸ� ��   �� ����� ��ũ��Ʈ, ������Ʈ�ѷ�.
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

    /// <summary>
    /// �浹 ó��. ���⼭ �� �Ӽ��� ���� ������� ��ȯ�Ѵ�.     // <---  ��ȯ�� void�ε� �ٲ������.
    /// </summary>
    public virtual void CollisionHandling()
    {
        
    }
}

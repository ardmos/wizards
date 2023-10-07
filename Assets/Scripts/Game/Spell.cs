using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// ������ �������� ��ɵ��� �����ϴ� Ŭ����
/// </summary>
public abstract class Spell : MonoBehaviour
{
    public SpellData spellData;

    /// <summary>
    /// �� ������ �����ϰ�(spellData)�� ���� ���ε��� ��ӹ޾Ƽ� ����
    /// </summary>
    public virtual void InitSpellDetail() {
        spellData = new SpellData();
    }

    /// <summary>
    /// �� �÷��̾��� ���� ĳ��Ʈ ��Ʈ�ѷ��� Update �޼ҵ忡�� �Է�ó�� ����� ���� ȣ��� �޼ҵ�.
    /// </summary>
    /// <param name="muzzle"> ������ �߻�� ��ġ </param>
    public virtual void CastSpell(Transform muzzle)
    {
        // �ؾ��� ��
        // 1. ���� ��ġ
        // 2. ���� ����(ȸ����)
        // 3. ���� ����

        if (!spellData.castAble)
        {
            spellData.restTime += Time.deltaTime;
            if (spellData.restTime >= spellData.coolTime)
            {
                spellData.castAble = true;
                spellData.restTime = 0f;
            }
        }
        if (spellData.castAble)  // inputSystem�� ���� �Է� üũ�� MagicSpellCastController���� �մϴ�. ��ų ��ư�� �����̱� ����.<---  �� ����� ��ũ��Ʈ, ������Ʈ�ѷ�.
        {
            GameObject spellObject = Instantiate(spellData.spellObjectPref);
            spellObject.transform.position = muzzle.position;

            spellData.castAble = false;
        }
    }

    /// <summary>
    /// �浹 ó��. ���⼭ �� �Ӽ��� ���� ������� ��ȯ�Ѵ�.   <--- ���⼭ �ϸ� ����� �ߺ��Ǵµ�? GameManager���� ����?
    /// </summary>
    public virtual SpellData CollisionHandling(SpellData playerSpellData, SpellData opponentsSpellData)
    {
        // 1. ���� ���
        CalcCollisionSpellLevel(playerSpellData.level, opponentsSpellData.level);
        // 2. ���� ���� ����

        // 3. ���� ������ ������ ��� �� ��ġ�� ���� ����

        switch (playerSpellData.spellType)
        {
            case SpellData.SpellType.Fire:
                // �� none
                // �� 
                break;
            case SpellData.SpellType.Water:
                break;
            case SpellData.SpellType.Ice:
                break;
            case SpellData.SpellType.Lightning:
                break;
            case SpellData.SpellType.Arcane:
                break;
            default:
                break;
        }


        return playerSpellData;
    }

    private int CalcCollisionSpellLevel(int playerSpellLevel, int opponentsSpellLevel)
    {
        int result = playerSpellLevel - opponentsSpellLevel;

        return result;
    }
}

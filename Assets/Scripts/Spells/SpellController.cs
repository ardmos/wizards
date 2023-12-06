using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// �÷��̾� ĳ���� ������Ʈ�� ���̴� ��ũ��Ʈ
/// ���� ���
///   1. ���� ���� ��Ȳ ����
///   2. ��ų �ߵ� 
///   3. ���� ���� ������ ���� ������ ����
/// </summary>
public class SpellController : MonoBehaviour
{
    [SerializeField] private GameObject[] currentSpellPrefabArray;
    [SerializeField] private List<Spell.SpellInfo> currentSpellInfoList;
    [SerializeField] private Player player;
    [SerializeField] private Transform muzzle;
    [SerializeField] private float restTimeCurrentSpell_1 = 0f;
    [SerializeField] private float restTimeCurrentSpell_2 = 0f;
    [SerializeField] private float restTimeCurrentSpell_3 = 0f;

    void Start()
    {
        // �� ���纰 ���� �ʱ�ȭ
        foreach (var spell in currentSpellPrefabArray)
        {
            if (spell != null)
            {
                spell.GetComponent<Spell>().InitSpellInfoDetail();
                currentSpellInfoList.Add(spell.GetComponent<Spell>().spellInfo);
            }
        }
    }

    void Update()
    {
        CheckCastSpell(1);
        CheckCastSpell(2);
        CheckCastSpell(3);
    }



    #region ���� ������ ���� ����
    private void CheckCastSpell(int spellNumber)
    {
        if (currentSpellPrefabArray[spellNumber - 1] == null) return;
        // ��Ÿ�� ����
        if (currentSpellInfoList[spellNumber - 1].castAble == false)
        {
            restTimeCurrentSpell_1 += Time.deltaTime;
            if (restTimeCurrentSpell_1 >= currentSpellInfoList[spellNumber - 1].coolTime)
            {
                currentSpellInfoList[spellNumber - 1].castAble = true;
                restTimeCurrentSpell_1 = 0f;
            }
            return;
        }

        // ��ų �ߵ� Ű �Է� ����
        bool isPlayerInput = false;
        switch (spellNumber)
        {
            case 1:
                isPlayerInput = player.IsAttack1();
                break;
            case 2:
                isPlayerInput = player.IsAttack2();
                break;
            case 3:
                isPlayerInput = player.IsAttack3();
                break;
            default:
                Debug.Log("CheckCastSpell : Wrong spellNumber");
                break;
        }

        if (isPlayerInput)
        {
            currentSpellPrefabArray[spellNumber - 1].GetComponent<Spell>().CastSpell(new Spell.SpellLvlType { level = currentSpellInfoList[spellNumber - 1].level, spellType = currentSpellInfoList[spellNumber - 1].spellType }, muzzle);
            currentSpellInfoList[spellNumber - 1].castAble = false;
        }
    }
    #endregion

    #region ���� ���� �̸� ���
    public string GetCurrentSpellName(int spellNumber)
    {
        if (currentSpellInfoList[spellNumber - 1] == null) return "";
        return currentSpellInfoList[spellNumber - 1].spellName;
    }
    #endregion

    #region ���� ���� �̹��� ���
    public Sprite GetCurrentSpellIcon(int spellNumber)
    {
        if (currentSpellInfoList[spellNumber - 1] == null) return null;
        return currentSpellInfoList[spellNumber - 1].iconImage;
    }
    #endregion

    #region ���� ���� restTime/coolTime ���
    public float GetCurrentSpellCoolTimeRatio(int spellNumber)
    {
        if (currentSpellInfoList[spellNumber - 1] == null) return 0f;
        float coolTimeRatio = restTimeCurrentSpell_1 / currentSpellInfoList[spellNumber - 1].coolTime;
        return coolTimeRatio;
    }
    #endregion

    #region ���� ���� ����
    public void SetCurrentSpell(GameObject spellObjectPrefab, int spellNumber)
    {
        currentSpellPrefabArray[spellNumber - 1] = spellObjectPrefab;
        currentSpellPrefabArray[spellNumber - 1].GetComponent<Spell>().InitSpellInfoDetail();
        currentSpellInfoList[spellNumber - 1] = currentSpellPrefabArray[spellNumber - 1].GetComponent<Spell>().spellInfo;
    }
    #endregion
}

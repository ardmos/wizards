using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// PlayerProfileData 
/// (�׽�Ʈ��. ���� ������ �̿� ������ �����Ͽ������Ͽ����� ������)
/// 1. ���� ���õ� ĳ���� ���� ����(Ŭ����, ������ ����&���� ���� ��)
/// 2. ������ ������� ���嵵 �ؾ���.
/// </summary>
public class PlayerProfileData : MonoBehaviour
{
    public static PlayerProfileData Instance { get; private set; }

    [SerializeField] private CharacterClasses.Class currentSelectedClass;

    private void Start()
    {
        Instance = this;
    }

    public CharacterClasses.Class GetCurrentSelectedClass() { return currentSelectedClass; } 
    
    public void SetCurrentSelectedClass(CharacterClasses.Class @class) { currentSelectedClass = @class; }

    public GameObject GetCurrentSelectedClassObjectForLobby()
    {
        // ������ ���⼭ ������� �ݿ��ؼ� ��ȯ�������. ������ Ŭ������ �ݿ��ؼ� ��ȯ����
        GameObject resultObejct = null;
        switch (currentSelectedClass)
        {
            case CharacterClasses.Class.Wizard:
                resultObejct = GameAssets.instantiate.wizard_Male_ForLobby;
                break;
            case CharacterClasses.Class.Knight:
                resultObejct = GameAssets.instantiate.knight_Male_ForLobby;
                break;
            default:
                break;
        }

        return resultObejct;
    }
    public GameObject GetCurrentSelectedClassObject()
    {
        // ������ ���⼭ ������� �ݿ��ؼ� ��ȯ�������. ������ Ŭ������ �ݿ��ؼ� ��ȯ����
        GameObject resultObejct = null;
        switch (currentSelectedClass)
        {
            case CharacterClasses.Class.Wizard:
                resultObejct = GameAssets.instantiate.wizard_Male;
                break;
            case CharacterClasses.Class.Knight:
                resultObejct = GameAssets.instantiate.knight_Male;
                break;
            default:
                break;
        }

        return resultObejct;
    }
}

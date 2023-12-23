using UnityEngine;
/// <summary>
/// Server Allocated �Ǳ� ������ Client������ PlayerClass ���� �������ִ� Ŭ����
/// ���� ����� GameMultiplayer���� ������.
/// PlayerProfileData 
/// (�׽�Ʈ��. ����� UGS Multiplay ������ �����. ���� UGS Cloud������ ���� ������ ����<<---�� �κ� ������ ����. Ready���� ��ε�ĳ���� �ϴ� �� ����, �̿� ������ �����Ͽ������Ͽ����� ��������)
/// 1. ���� ���õ� ĳ���� ���� ����(Ŭ����, ������ ����&���� ���� ��)
/// 2. ������ ������� ���嵵 �ؾ���.
/// </summary>
public class PlayerProfileData : MonoBehaviour
{
    public static PlayerProfileData Instance { get; private set; }

    [SerializeField] private CharacterClasses.Class currentSelectedClass;

    private void Start()
    {
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("PlayerProfileData");
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public CharacterClasses.Class GetCurrentSelectedClass() 
    {
        return currentSelectedClass; 
    }

    // Lobby Scene ����. Client ���� �����
    public void SetCurrentSelectedClass(CharacterClasses.Class @class) { currentSelectedClass = @class; }

    // Lobby Scene ����. Client ���� �����
    public GameObject GetCurrentSelectedCharacterPrefab_NotInGame()
    {
        //Debug.Log($"GetCurrentSelectedCharacterPrefab_NotInGame currentSelectedClass: {currentSelectedClass}");
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
        //Debug.Log($"GetCurrentSelectedCharacterPrefab_NotInGame resultObject: {resultObejct?.name}");
        return resultObejct;
    }

    // Lobby Scene ����. Client ���� �����
    public GameObject GetCurrentSelectedCharacterPrefab_InGame()
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

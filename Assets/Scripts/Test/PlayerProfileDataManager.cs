using UnityEngine;
/// <summary>
/// Server Allocated �Ǳ� ������ Client������ PlayerClass ���� �������ִ� Ŭ����
/// ���� ����� GameMultiplayer���� ������.
/// PlayerProfileData 
/// (�׽�Ʈ��. ����� UGS Multiplay ������ �����. ���� UGS Cloud������ ���� ������ ����<<---�� �κ� ������ ����. Ready���� ��ε�ĳ���� �ϴ� �� ����, �̿� ������ �����Ͽ������Ͽ����� ��������)
/// 1. ���� ���õ� ĳ���� ���� ����(Ŭ����, ������ ����&���� ���� ��)
/// 2. ������ ������� ���嵵 �ؾ���.
/// </summary>
public class PlayerProfileDataManager : MonoBehaviour
{
    public static PlayerProfileDataManager Instance { get; private set; }

    [SerializeField] private PlayerData playerData;

    private void Awake()
    {
        // Need to be Saved!!!!!! in Application Data.   Not New!!!      Choo hoo --> on Server.
        //playerData =     From Here!!!!

    }

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

    public void SetPlayerName(string newName)
    {
        playerData.playerName = newName;
    }

    public string GetPlayerName()
    {
        return playerData.playerName.ToString();
    }

    // Lobby Scene ����. Client ���� �����
    public void SetCurrentSelectedClass(CharacterClass characterClass) { playerData.playerClass = characterClass; }

    public CharacterClass GetCurrentSelectedClass() 
    {
        return playerData.playerClass; 
    }

    public PlayerData GetPlayerData()
    {
        return playerData;
    }
}

using Unity.Services.Matchmaker.Models;
using UnityEngine;
/// <summary>
/// Server Allocated �Ǳ� ������ Client������ PlayerClass ���� �������ִ� Ŭ����
/// ���� ����� GameMultiplayer���� ������.
/// PlayerProfileData 
/// (�׽�Ʈ��. ����� UGS Multiplay ������ �����. ���� UGS Cloud������ ���� ������ ����<<---�� �κ� ������ ����. Ready���� ��ε�ĳ���� �ϴ� �� ����, �̿� ������ �����Ͽ������Ͽ����� ��������)
/// 1. ���� ���õ� ĳ���� ���� ����(Ŭ����, ������ ����&���� ���� ��)
/// 2. ������ ������� ���嵵 �ؾ���.
/// 
/// Player ��ũ��Ʈ�� PlayerData�� ��ĥ �ʿ䰡 ����. 
/// </summary>
public class PlayerProfileDataManager : MonoBehaviour
{
    public static PlayerProfileDataManager Instance { get; private set; }

    [SerializeField] private PlayerData playerData;

    private void Awake()
    {
        // Need to be Saved!!!!!! in Application Data.   Not New!!!      Choo hoo --> on Server.
        //playerData =     From Here!!!!
        //playerId  = UnityAuthenticationManager�κ��� ���ͼ� ����. 
        // �Ʒ� �������� ���� ���ص� ��. �ϴ� ������ ���ÿ� �����͸� �����ؼ� �����ϸ� �� ���ư��� �����صΰ�,
        // ���Ŀ� ���� �����ظ� ������ ������ �����ϵ��� ������Ʈ ���ָ��. 
        // Title�������� 1.�α׾ƿ����°� false�϶�, �̹� ����� playerData������ �ִ� ���, �α��ΰ��� ����. �׳� �ٷ� ���� 
        // Lobby�������� �α׾ƿ� ��� ����. �α׾ƿ� state�� true�� ����, Title������ �̵�. �ڿ����� �α��� ���� ����. 


        LoadPlayerData();
    }

    // Player ������ ���⼭ ���̺�&�ε� ����???  �̹� ���̺�ý����� �߰��Ǿ���, Ȱ�� �κ��� �����ϸ� �˴ϴ�.

    /// <summary>
    /// �κ񿡼� Player���� ���� Ÿ�̹�.
    /// 1. Player ������ �����Ǿ��� ��.
    /// 
    /// Player���� �ε� Ÿ�̹�.
    /// 1. �κ���� ������ ��.
    /// 
    /// 
    /// </summary>

    // 1. Player���� ���� <<<<<<<<-------------- �̰� Ȱ���ϴ� �κк��� �ϸ� �˴ϴ�.
    private void SavePlayerData()
    {
        SaveSystem.SavePlayerData(playerData);
    }
    public void UpdatePlayerData(PlayerData playerData) // �� �޼ҵ� �ʿ� ���� �Ʒ����� Set�̸�, Ŭ���� �� �� SavePlayerData() ȣ���ϴ¹���̸� ���? �� ����Ѱɷ� ��������.
    {
        this.playerData = playerData;
        SavePlayerData();
    }

    // 2. Player���� �ҷ�����
    private void LoadPlayerData()
    {
        PlayerDataForSave playerDataForSave = SaveSystem.LoadPlayerData(UnityAuthenticationManager.Instance.GetPlayerID());
        if (playerDataForSave != null)
        {
            playerData = new PlayerData()
            {
                playerLevel = playerDataForSave.playerLevel,
                playerId = playerDataForSave.playerId,
                playerName = playerDataForSave.playerName,
                characterClass = playerDataForSave.characterClass
            };
        }
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
    public void SetCurrentPlayerClass(CharacterClass characterClass) 
    { 
        playerData.characterClass = characterClass; 
    }

    public CharacterClass GetCurrentPlayerClass() 
    {
        return playerData.characterClass; 
    }

    public PlayerData GetPlayerData()
    {
        return playerData;
    }
}

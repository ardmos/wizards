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
public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager Instance { get; private set; }

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

#if !UNITY_SERVER
        if (!LoadPlayerData())
        {
            // �ε��� �÷��̾� ������ ������ ���. 
            // ���� �����͸� ������ش�.
            playerData = new PlayerData(
                new PlayerInGameData()
                {
                    playerName = "Player" 
                }
                ,
                new PlayerOutGameData()
                );
            SavePlayerData();
        }
#endif
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

    // 1. Player���� ����
    private void SavePlayerData()
    {
        try
        {
            SaveSystem.SavePlayerData(playerData);
            Debug.Log($"�÷��̾� ���� ���̺� ����!");
        }
        catch (System.Exception)
        {
            throw;
        }
    }

    // ���� ����� ������ �� ����ϴ� �޼���.
    public void UpdatePlayerData(PlayerOutGameData playerOutGameData)
    {
        playerData.playerOutGameData = playerOutGameData;
        SaveSystem.SavePlayerData(playerData);
    }

    // 2. Player���� �ҷ�����
    /// <summary>
    /// �������θ� ��ȯ�մϴ�.
    /// </summary>
    /// <returns></returns>
    private bool LoadPlayerData()
    {
        PlayerData playerData = SaveSystem.LoadPlayerData();
        if (playerData != null)
        {
            Debug.Log($"�÷��̾� ���� �ε� ����!");
            this.playerData = playerData;
            return true;
        }
        else
        {
            Debug.Log($"�ε��� �÷��̾� ������ �����ϴ�.");
            return false;
        }
    }

    private void Start()
    {
        if (Instance == null)
        {
            Instance = this;
            //Debug.Log("PlayerProfileData");
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void UpdatePlayerName(string newName)
    {
        playerData.playerInGameData.playerName = newName;
        SavePlayerData();
    }

    public string GetPlayerName()
    {
        return playerData.playerInGameData.playerName.ToString();
    }

    // Lobby Scene ����. Client ���� �����
    public void SetCurrentPlayerClass(Character characterClass) 
    { 
        playerData.playerInGameData.characterClass = characterClass;
        SavePlayerData();
    }

    public Character GetCurrentPlayerClass() 
    {
        return playerData.playerInGameData.characterClass; 
    }

    public PlayerInGameData GetPlayerInGameData()
    {
        return playerData.playerInGameData;
    }
    public PlayerOutGameData GetPlayerOutGameData()
    {
        return playerData.playerOutGameData;
    }

    public PlayerData GetPlayerData()
    {
        return playerData;
    }
}

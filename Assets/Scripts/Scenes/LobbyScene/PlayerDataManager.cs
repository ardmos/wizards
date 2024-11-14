using Unity.Services.Matchmaker.Models;
using UnityEngine;
/// <summary>
/// Server Allocated 되기 전까지 Client측에서 PlayerClass 정보 관리해주는 클래스
/// 서버 생기면 GameMultiplayer에서 관리함.
/// PlayerProfileData 
/// (테스트용. 현재는 UGS Multiplay 서버를 사용함. 추후 UGS Cloud서버에 계정 정보를 저장<<---이 부분 구현할 차례. Ready정보 브로드캐스팅 하는 것 참고, 이용 계정과 연동하여관리하여야할 데이터임)
/// 1. 현재 선택된 캐릭터 정보 저장(클래스, 아이템 장착&보유 상태 등)
/// 2. 원래는 복장상태 저장도 해야함.
/// 
/// Player 스크립트의 PlayerData로 합칠 필요가 있음. 
/// </summary>
public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager Instance { get; private set; }

    [SerializeField] private PlayerData playerData;

    private void Awake()
    {
        // Need to be Saved!!!!!! in Application Data.   Not New!!!      Choo hoo --> on Server.
        //playerData =     From Here!!!!
        //playerId  = UnityAuthenticationManager로부터 얻어와서 저장. 
        // 아래 과정들은 구현 안해도 됨. 일단 지금은 로컬에 데이터를 저장해서 삭제하면 다 날아가게 구현해두고,
        // 추후에 서버 구현해면 서버에 데이터 저장하도록 업데이트 해주면됨. 
        // Title씬에서는 1.로그아웃상태가 false일때, 이미 저장된 playerData파일이 있는 경우, 로그인과정 생략. 그냥 바로 시작 
        // Lobby씬에서는 로그아웃 기능 구현. 로그아웃 state를 true로 변경, Title씬으로 이동. 자연스레 로그인 과정 시작. 

#if !UNITY_SERVER
        if (!LoadPlayerData())
        {
            // 로드할 플레이어 정보가 없었던 경우. 
            // 최초 데이터를 만들어준다.
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

    // Player 정보를 여기서 세이브&로드 할지???  이미 세이브시스템은 추가되었고, 활용 부분을 정리하면 됩니다.

    /// <summary>
    /// 로비에서 Player정보 저장 타이밍.
    /// 1. Player 정보가 수정되었을 때.
    /// 
    /// Player정보 로드 타이밍.
    /// 1. 로비씬이 열렸을 때.
    /// 
    /// 
    /// </summary>

    // 1. Player정보 저장
    private void SavePlayerData()
    {
        try
        {
            SaveSystem.SavePlayerData(playerData);
            Debug.Log($"플레이어 정보 세이브 성공!");
        }
        catch (System.Exception)
        {
            throw;
        }
    }

    // 게임 결과를 저장할 때 사용하는 메서드.
    public void UpdatePlayerData(PlayerOutGameData playerOutGameData)
    {
        playerData.playerOutGameData = playerOutGameData;
        SaveSystem.SavePlayerData(playerData);
    }

    // 2. Player정보 불러오기
    /// <summary>
    /// 성공여부를 반환합니다.
    /// </summary>
    /// <returns></returns>
    private bool LoadPlayerData()
    {
        PlayerData playerData = SaveSystem.LoadPlayerData();
        if (playerData != null)
        {
            Debug.Log($"플레이어 정보 로드 성공!");
            this.playerData = playerData;
            return true;
        }
        else
        {
            Debug.Log($"로드할 플레이어 정보가 없습니다.");
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

    // Lobby Scene 전용. Client 내부 저장용
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

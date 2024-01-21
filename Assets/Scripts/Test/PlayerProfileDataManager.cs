using UnityEngine;
/// <summary>
/// Server Allocated 되기 전까지 Client측에서 PlayerClass 정보 관리해주는 클래스
/// 서버 생기면 GameMultiplayer에서 관리함.
/// PlayerProfileData 
/// (테스트용. 현재는 UGS Multiplay 서버를 사용함. 추후 UGS Cloud서버에 계정 정보를 저장<<---이 부분 구현할 차례. Ready정보 브로드캐스팅 하는 것 참고, 이용 계정과 연동하여관리하여야할 데이터임)
/// 1. 현재 선택된 캐릭터 정보 저장(클래스, 아이템 장착&보유 상태 등)
/// 2. 원래는 복장상태 저장도 해야함.
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

    // Lobby Scene 전용. Client 내부 저장용
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

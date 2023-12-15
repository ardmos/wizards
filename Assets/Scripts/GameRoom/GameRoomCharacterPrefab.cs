using Unity.Netcode;
using UnityEngine;
/// <summary>
/// GameRoom씬에서 사용되는 Player 캐릭터 프리팹의 스크립트로
/// 각 Player들이 선택한 영웅의 3D Visual 프리팹을 자식오브젝트로 가져와 보여주는 역할을 한다. 
/// 1. Player들이 선택한 영웅의 Prefab 가져와 Prefab을 자식 오브젝트로 인스턴시에이트 하기
/// </summary>
public class GameRoomCharacterPrefab : MonoBehaviour
{
    [SerializeField] private GameObject playerObject;
    /// <summary>
    /// Player가 선택한 캐릭터의 비주얼을 가져와 보여줍니다.
    /// 총 과정 
    /// 1. 로비씬에서 PlayerProfileData에 선택중인 클래스를 저장
    /// 2. 서버가 열리면 GameMultiplayer의 NetworkList인 playerDataList에 저장.
    /// 3. 필요할 때 playerIndex나 clientID로 꺼내서 사용.
    /// </summary>
    public void ShowCharacter3DVisual(int playerIndex)
    {        
        //Debug.Log($"LoadCharacter3DVisual playerIndex:{playerIndex}, playerObject:{playerObject}");
        playerObject = Instantiate(GameMultiplayer.Instance.GetPlayerClassPrefabByPlayerIndex_NotForGameSceneObject(playerIndex));
        playerObject.transform.SetParent(transform, false);
        playerObject.transform.localPosition = Vector3.zero;
    }

    public void HideCharacter3DVisual() {
        Destroy(playerObject);
        playerObject = null;
    }
}

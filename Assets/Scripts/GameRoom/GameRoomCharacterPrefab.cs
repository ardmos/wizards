using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// GameRoom씬에서 사용되는 Player 캐릭터 프리팹의 스크립트로
/// 각 Player들이 선택한 영웅의 3D Visual 프리팹을 자식오브젝트로 가져와 보여주는 역할을 한다. 
/// 1. Player들이 선택한 영웅의 Prefab 가져와 Prefab을 자식 오브젝트로 인스턴시에이트 하기
/// </summary>
public class GameRoomCharacterPrefab : MonoBehaviour
{
    [SerializeField] GameObject playerObject;
    /// <summary>
    /// Player가 선택한 캐릭터의 비주얼을 가져와 보여줍니다.
    /// </summary>
    public void UpdateCharacter3DVisual()
    {
        if(playerObject != null) Destroy(playerObject);

        Debug.Log($"LoadCharacter3DVisual");
        playerObject = Instantiate(PlayerProfileData.Instance.GetCurrentSelectedCharacterPrefab_NotInGame());
        playerObject.transform.SetParent(transform, false);
        playerObject.transform.localPosition = Vector3.zero;
    }
}

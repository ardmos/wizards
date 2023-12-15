using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// GameRoom������ ���Ǵ� Player ĳ���� �������� ��ũ��Ʈ��
/// �� Player���� ������ ������ 3D Visual �������� �ڽĿ�����Ʈ�� ������ �����ִ� ������ �Ѵ�. 
/// 1. Player���� ������ ������ Prefab ������ Prefab�� �ڽ� ������Ʈ�� �ν��Ͻÿ���Ʈ �ϱ�
/// </summary>
public class GameRoomCharacterPrefab : MonoBehaviour
{
    [SerializeField] GameObject playerObject;
    /// <summary>
    /// Player�� ������ ĳ������ ���־��� ������ �����ݴϴ�.
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

using Unity.Netcode;
using UnityEngine;
/// <summary>
/// GameRoom������ ���Ǵ� Player ĳ���� �������� ��ũ��Ʈ��
/// �� Player���� ������ ������ 3D Visual �������� �ڽĿ�����Ʈ�� ������ �����ִ� ������ �Ѵ�. 
/// 1. Player���� ������ ������ Prefab ������ Prefab�� �ڽ� ������Ʈ�� �ν��Ͻÿ���Ʈ �ϱ�
/// </summary>
public class GameRoomCharacterPrefab : MonoBehaviour
{
    [SerializeField] private GameObject playerObject;
    /// <summary>
    /// Player�� ������ ĳ������ ���־��� ������ �����ݴϴ�.
    /// �� ���� 
    /// 1. �κ������ PlayerProfileData�� �������� Ŭ������ ����
    /// 2. ������ ������ GameMultiplayer�� NetworkList�� playerDataList�� ����.
    /// 3. �ʿ��� �� playerIndex�� clientID�� ������ ���.
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

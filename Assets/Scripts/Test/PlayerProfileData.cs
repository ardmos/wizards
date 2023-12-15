using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
/// <summary>
/// PlayerProfileData 
/// (테스트용. 현재는 UGS Multiplay 서버를 사용함. 추후 UGS Cloud서버에 계정 정보를 저장, 이용 계정과 연동하여관리하여야할 데이터임)
/// 1. 현재 선택된 캐릭터 정보 저장(클래스, 아이템 장착&보유 상태 등)
/// 2. 원래는 복장상태 저장도 해야함.
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
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public CharacterClasses.Class GetCurrentSelectedClass() { return currentSelectedClass; } 
    
    public void SetCurrentSelectedClass(CharacterClasses.Class @class) { currentSelectedClass = @class; }

    public GameObject GetCurrentSelectedCharacterPrefab_NotInGame()
    {
        Debug.Log($"GetCurrentSelectedCharacterPrefab_NotInGame currentSelectedClass: {currentSelectedClass}");
        // 원래는 여기서 복장상태 반영해서 반환해줘야함. 지금은 클래스만 반영해서 반환해줌
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
        Debug.Log($"GetCurrentSelectedCharacterPrefab_NotInGame resultObject: {resultObejct?.name}");
        return resultObejct;
    }
    public GameObject GetCurrentSelectedCharacterPrefab_InGame()
    {
        // 원래는 여기서 복장상태 반영해서 반환해줘야함. 지금은 클래스만 반영해서 반환해줌
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

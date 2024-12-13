using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using static ComponentValidator;

/// <summary>
/// 씬 정리를 관리하는 클래스입니다.
/// ICleanable 인터페이스를 구현한 객체들을 등록하고 정리하는 기능을 제공합니다.
/// </summary>
public static class SceneCleanupManager
{
    #region Constants & Fields
    private const string ERROR_CLEANABLE_NOT_SET = "SceneCleanupManager cleanable 객체가 설정되지 않았습니다.";
    private const string ERROR_NETWORK_MANAGER_NOT_SET = "SceneCleanupManager NetworkManager.Singleton이 설정되지 않았습니다.";

    // 정리 대상 객체들을 저장하는 리스트
    private static List<ICleanable> cleanableObjects = new List<ICleanable>();
    #endregion

    #region Object Registration
    /// <summary>
    /// 정리 대상 객체를 등록합니다.
    /// </summary>
    /// <param name="cleanable">등록할 ICleanable 객체</param>
    public static void RegisterCleanableObject(ICleanable cleanable)
    {
        if (!ValidateComponent(cleanable, ERROR_CLEANABLE_NOT_SET)) return;

        cleanableObjects.Add(cleanable);
    }

    /// <summary>
    /// 등록된 정리 대상 객체를 제거합니다.
    /// </summary>
    /// <param name="cleanable">제거할 ICleanable 객체</param>
    public static void UnregisterCleanableObject(ICleanable cleanable)
    {
        if (!ValidateComponent(cleanable, ERROR_CLEANABLE_NOT_SET)) return;

        cleanableObjects.Remove(cleanable);
    }
    #endregion

    #region Cleanup Operations
    /// <summary>
    /// 모든 등록된 객체의 Cleanup 메서드를 호출하고, NetworkManager를 정리합니다.
    /// </summary>
    public static void CleanupAllObjects()
    {
        CleanupRegisteredObjects();
        DestroyNetworkManager();
    }

    /// <summary>
    /// 등록된 모든 객체의 Cleanup 메서드를 호출합니다.
    /// </summary>
    private static void CleanupRegisteredObjects()
    {
        // 리스트를 복사하여 순회 중 리스트 변경 문제를 방지합니다.
        var cleanablesToProcess = cleanableObjects.ToList();
        foreach (var cleanable in cleanablesToProcess)
        {
            if (!ValidateComponent(cleanable, ERROR_CLEANABLE_NOT_SET)) continue;

            try
            {
                cleanable.Cleanup();
            }
            catch (System.Exception ex)
            {
                Logger.LogError($"{cleanable}오브젝트의 Cleanup() 작업을 실패했습니다 : {ex.Message}");
            }
        }
        cleanableObjects.Clear();
    }

    /// <summary>
    /// NetworkManager를 종료하고 제거합니다.
    /// </summary>
    private static void DestroyNetworkManager()
    {
        if (!ValidateComponent(NetworkManager.Singleton, ERROR_NETWORK_MANAGER_NOT_SET)) return;

        try
        {
            NetworkManager.Singleton.Shutdown();
            GameObject.Destroy(NetworkManager.Singleton.gameObject);
        }
        catch (System.Exception ex)
        {
            Logger.LogError($"NetworkManager 종료 및 제거 작업을 실패했습니다 : {ex.Message}");
        }
    }
    #endregion
}
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using static ComponentValidator;

/// <summary>
/// �� ������ �����ϴ� Ŭ�����Դϴ�.
/// ICleanable �������̽��� ������ ��ü���� ����ϰ� �����ϴ� ����� �����մϴ�.
/// </summary>
public static class SceneCleanupManager
{
    #region Constants & Fields
    private const string ERROR_CLEANABLE_NOT_SET = "SceneCleanupManager cleanable ��ü�� �������� �ʾҽ��ϴ�.";
    private const string ERROR_NETWORK_MANAGER_NOT_SET = "SceneCleanupManager NetworkManager.Singleton�� �������� �ʾҽ��ϴ�.";

    // ���� ��� ��ü���� �����ϴ� ����Ʈ
    private static List<ICleanable> cleanableObjects = new List<ICleanable>();
    #endregion

    #region Object Registration
    /// <summary>
    /// ���� ��� ��ü�� ����մϴ�.
    /// </summary>
    /// <param name="cleanable">����� ICleanable ��ü</param>
    public static void RegisterCleanableObject(ICleanable cleanable)
    {
        if (!ValidateComponent(cleanable, ERROR_CLEANABLE_NOT_SET)) return;

        cleanableObjects.Add(cleanable);
    }

    /// <summary>
    /// ��ϵ� ���� ��� ��ü�� �����մϴ�.
    /// </summary>
    /// <param name="cleanable">������ ICleanable ��ü</param>
    public static void UnregisterCleanableObject(ICleanable cleanable)
    {
        if (!ValidateComponent(cleanable, ERROR_CLEANABLE_NOT_SET)) return;

        cleanableObjects.Remove(cleanable);
    }
    #endregion

    #region Cleanup Operations
    /// <summary>
    /// ��� ��ϵ� ��ü�� Cleanup �޼��带 ȣ���ϰ�, NetworkManager�� �����մϴ�.
    /// </summary>
    public static void CleanupAllObjects()
    {
        CleanupRegisteredObjects();
        DestroyNetworkManager();
    }

    /// <summary>
    /// ��ϵ� ��� ��ü�� Cleanup �޼��带 ȣ���մϴ�.
    /// </summary>
    private static void CleanupRegisteredObjects()
    {
        // ����Ʈ�� �����Ͽ� ��ȸ �� ����Ʈ ���� ������ �����մϴ�.
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
                Logger.LogError($"{cleanable}������Ʈ�� Cleanup() �۾��� �����߽��ϴ� : {ex.Message}");
            }
        }
        cleanableObjects.Clear();
    }

    /// <summary>
    /// NetworkManager�� �����ϰ� �����մϴ�.
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
            Logger.LogError($"NetworkManager ���� �� ���� �۾��� �����߽��ϴ� : {ex.Message}");
        }
    }
    #endregion
}
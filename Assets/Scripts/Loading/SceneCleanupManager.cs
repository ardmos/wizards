using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class SceneCleanupManager:MonoBehaviour
{
    private static List<ICleanable> cleanableObjects = new List<ICleanable>();

    public static void RegisterCleanableObject(ICleanable cleanable)
    {
        cleanableObjects.Add(cleanable);
    }

    public static void UnregisterCleanableObject(ICleanable cleanable)
    {
        cleanableObjects.Remove(cleanable);
    }

    public static void CleanupAllObjects()
    {
        var cleanablesToProcess = cleanableObjects.ToList();
        foreach (var cleanable in cleanablesToProcess)
        {
            if (cleanable != null)
            {
                cleanable.Cleanup();
            }
        }
        cleanableObjects.Clear();

        DestroyNetworkManager();
    }

    private static void DestroyNetworkManager()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
            Destroy(NetworkManager.Singleton.gameObject);
        }
    }
}

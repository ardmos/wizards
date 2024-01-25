using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
/// <summary>
/// Ŭ���̾�Ʈ�� ������ ��� ������ �����ϱ� ������ ����ϴ� ��ũ��Ʈ�Դϴ�.
/// ���ÿ� Ŭ���̾�Ʈ�� ������ �������ݴϴ�.
/// </summary>
public static class SaveSystem
{
    // wak�� Wizards and Knights�� ���̺� ���� �ڷ���. �α��� ������ �ȵ� ������ ���ϸ��� �ϵ��ڵ��մϴ�.    
    private static string path = Application.persistentDataPath + $"/playerSaveData.wak";

    public static void SavePlayerData(PlayerData playerData)
    {
        BinaryFormatter binaryFormatter = new BinaryFormatter();
 
        FileStream fileStream = new FileStream(path, FileMode.Create);

        PlayerDataForSave data = new PlayerDataForSave(playerData);
        binaryFormatter.Serialize(fileStream, data);
        fileStream.Close();
    }

    public static PlayerDataForSave LoadPlayerData()
    {      
        if (File.Exists(path))
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            FileStream fileStream = new FileStream (path, FileMode.Open);

            PlayerDataForSave data = binaryFormatter.Deserialize(fileStream) as PlayerDataForSave;
            fileStream.Close();
            return data;
        }
        else
        {
            Debug.LogError($"Save file not found in {path}");
            return null;
        }
    }
}
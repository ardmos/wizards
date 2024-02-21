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

        //PlayerData data = playerData;
        SaveData saveData = new SaveData(playerData, SoundManager.instance.GetSoundVolumeData());
        binaryFormatter.Serialize(fileStream, saveData);
        fileStream.Close();
    }

    public static PlayerData LoadPlayerData()
    {      
        if (File.Exists(path))
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            FileStream fileStream = new FileStream (path, FileMode.Open);

            SaveData saveData = binaryFormatter.Deserialize(fileStream) as SaveData;
            PlayerData data = saveData.playerData;
            fileStream.Close();
            return data;
        }
        else
        {
            Debug.LogError($"Save file not found in {path}");
            return null;
        }
    }

    public static SoundVolumeData LoadSoundVolumeData()
    {
        //Debug.Log($"{path}");
        if (File.Exists(path))
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            FileStream fileStream = new FileStream(path, FileMode.Open);

            SaveData saveData = binaryFormatter.Deserialize(fileStream) as SaveData;
            SoundVolumeData data = saveData.soundVolumeData;
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
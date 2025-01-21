using System;
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
        using (FileStream fileStream = new FileStream(path, FileMode.Create))
        {
            try
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                SaveData saveData = new SaveData(playerData, SoundManager.Instance.GetSoundVolumeData(), new GraphicQualitySettingsData(GraphicQualityManager.Instance.GetQualityLevel()));
                binaryFormatter.Serialize(fileStream, saveData);
            }
            catch (Exception e)
            {
                Logger.LogError($"�÷��̾� �����͸� ���̺��ϴµ� ������ �߻��߽��ϴ�: {e.Message}");
            }
        }
    }

    public static void SaveSoundVolumeData(SoundVolumeData soundVolumeData)
    {
        using (FileStream fileStream = new FileStream(path, FileMode.Create))
        {
            try
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                SaveData saveData = new SaveData(LocalPlayerDataManagerClient.Instance.GetPlayerData(), soundVolumeData, new GraphicQualitySettingsData(GraphicQualityManager.Instance.GetQualityLevel()));
                binaryFormatter.Serialize(fileStream, saveData);
            }
            catch (Exception e)
            {
                Logger.LogError($"���� ���� �����͸� ���̺��ϴµ� ������ �߻��߽��ϴ�: {e.Message}");
            }
        }
    }

    public static void SaveGrahpicQualitySettingsData(GraphicQualitySettingsData graphicQualitySettingsData)
    {
        using (FileStream fileStream = new FileStream(path, FileMode.Create))
        {
            try
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                SaveData saveData = new SaveData(LocalPlayerDataManagerClient.Instance.GetPlayerData(), SoundManager.Instance.GetSoundVolumeData(), graphicQualitySettingsData);
                binaryFormatter.Serialize(fileStream, saveData);
            }
            catch (Exception e)
            {
                Logger.LogError($"�׷��� ���� �����͸� ���̺��ϴµ� ������ �߻��߽��ϴ�: {e.Message}");
            }
        }
    }

    public static T LoadData <T>() where T : class
    {
        if (!File.Exists(path))
        {
            Logger.LogError($"���̺������� �߰����� ���߽��ϴ�: {path}");
            return null;
        }

        using (FileStream fileStream = new FileStream(path, FileMode.Open))
        {
            if (fileStream.Length <= 0)
            {
                Logger.LogError("���̺� ������ �ջ�ƽ��ϴ�. ������ �����մϴ�");
                fileStream.Close();
                File.Delete(path);
                if (!File.Exists(path))
                    Logger.LogError("���� ����");
                else
                    Logger.LogError("���� ����");
                return null;
            }

            // ���̺����� �ε�
            try
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                SaveData saveData = binaryFormatter.Deserialize(fileStream) as SaveData;

                if(typeof(T) == typeof(SoundVolumeData))
                {
                    return saveData.soundVolumeData as T;
                }
                else if(typeof(T) == typeof(GraphicQualitySettingsData))
                {
                    return saveData.graphicQualitySettingsData as T;
                }
                else if (typeof(T) == typeof(PlayerData))
                {
                    return saveData.playerData as T;
                }
                else
                {
                    Logger.LogError($"�������� �ʴ� ������ Ÿ���Դϴ�.");
                    return null;
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"���̺������� �о���µ� ������ �߻��߽��ϴ�: {e.Message}");
                return null;
            }            
        }

    }
}
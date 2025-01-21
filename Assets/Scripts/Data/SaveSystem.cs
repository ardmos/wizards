using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
/// <summary>
/// 클라이언트의 정보를 담는 서버를 구축하기 전까지 사용하는 스크립트입니다.
/// 로컬에 클라이언트의 정보를 저장해줍니다.
/// </summary>
public static class SaveSystem
{
    // wak는 Wizards and Knights의 세이브 파일 자료형. 로그인 구현이 안된 지금은 파일명을 하드코딩합니다.    
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
                Logger.LogError($"플레이어 데이터를 세이브하는데 문제가 발생했습니다: {e.Message}");
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
                Logger.LogError($"사운드 설정 데이터를 세이브하는데 문제가 발생했습니다: {e.Message}");
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
                Logger.LogError($"그래픽 설정 데이터를 세이브하는데 문제가 발생했습니다: {e.Message}");
            }
        }
    }

    public static T LoadData <T>() where T : class
    {
        if (!File.Exists(path))
        {
            Logger.LogError($"세이브파일을 발견하지 못했습니다: {path}");
            return null;
        }

        using (FileStream fileStream = new FileStream(path, FileMode.Open))
        {
            if (fileStream.Length <= 0)
            {
                Logger.LogError("세이브 파일이 손상됐습니다. 파일을 삭제합니다");
                fileStream.Close();
                File.Delete(path);
                if (!File.Exists(path))
                    Logger.LogError("삭제 성공");
                else
                    Logger.LogError("삭제 실패");
                return null;
            }

            // 세이브파일 로드
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
                    Logger.LogError($"지원하지 않는 데이터 타입입니다.");
                    return null;
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"세이브파일을 읽어오는데 문제가 발생했습니다: {e.Message}");
                return null;
            }            
        }

    }
}
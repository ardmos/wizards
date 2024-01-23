using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
/// <summary>
/// 클라이언트의 정보를 담는 서버를 구축하기 전까지 사용하는 스크립트입니다.
/// 로컬에 클라이언트의 정보를 저장해줍니다.
/// </summary>
public static class SaveSystem
{
    public static void SavePlayerData(PlayerData playerData)
    {
        BinaryFormatter binaryFormatter = new BinaryFormatter();

        // wak는 Wizards and Knights의 세이브 파일 자료형
        string path = Application.persistentDataPath + $"{playerData.playerId}.wak";
        FileStream fileStream = new FileStream(path, FileMode.Create);

        PlayerDataForSave data = new PlayerDataForSave(playerData);
        binaryFormatter.Serialize(fileStream, data);
        fileStream.Close();
    }

    public static PlayerDataForSave LoadPlayerData(string playerId)
    {
        string path = Application.persistentDataPath + $"{playerId}.wak";
        if(File.Exists(path))
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
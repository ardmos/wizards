using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
/// <summary>
/// Ŭ���̾�Ʈ�� ������ ��� ������ �����ϱ� ������ ����ϴ� ��ũ��Ʈ�Դϴ�.
/// ���ÿ� Ŭ���̾�Ʈ�� ������ �������ݴϴ�.
/// </summary>
public static class SaveSystem
{
    public static void SavePlayerData(PlayerData playerData)
    {
        BinaryFormatter binaryFormatter = new BinaryFormatter();

        // wak�� Wizards and Knights�� ���̺� ���� �ڷ���
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
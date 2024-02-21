[System.Serializable]
public class SaveData
{
    public PlayerData playerData;
    public SoundVolumeData soundVolumeData;

    public SaveData(PlayerData playerData, SoundVolumeData soundVolumeData)
    {
        this.playerData = playerData;
        this.soundVolumeData = soundVolumeData;
    }
}
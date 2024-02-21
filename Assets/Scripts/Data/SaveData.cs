[System.Serializable]
public class SaveData
{
    public PlayerData playerData;
    public SoundVolumeData soundVolumeData;
    public GraphicQualitySettingsData graphicQualitySettingsData;

    public SaveData(PlayerData playerData, SoundVolumeData soundVolumeData, GraphicQualitySettingsData graphicQualitySettingsData)
    {
        this.playerData = playerData;
        this.soundVolumeData = soundVolumeData;
        this.graphicQualitySettingsData = graphicQualitySettingsData;
    }
}
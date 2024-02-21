[System.Serializable]
public class SoundVolumeData
{
    public float BGMVolume;
    public float SFXVolume;

    public SoundVolumeData(float bgmVolume, float sfxVolume)
    {
        this.BGMVolume = bgmVolume;
        this.SFXVolume = sfxVolume;
    }

    public void UpdateData(float bgmVolume, float sfxVolume)
    {
        this.BGMVolume = bgmVolume;
        this.SFXVolume = sfxVolume;
    }
}
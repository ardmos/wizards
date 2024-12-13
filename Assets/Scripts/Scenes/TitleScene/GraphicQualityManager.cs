using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Cinemachine.DocumentationSortingAttribute;

public class GraphicQualityManager : MonoBehaviour
{
    public static GraphicQualityManager Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        LoadGraphicQualitySettingsData();
    }

    public void SetQualityLevel(int level)
    {
        QualitySettings.SetQualityLevel(level, true);
        Debug.Log($"그래픽 퀄리티 설정 적용 성공!");

        // SaveData에 저장.
        SaveGrahpicQualitySettingsData(new GraphicQualitySettingsData(level));
    }

    public int GetQualityLevel()
    {
        return QualitySettings.GetQualityLevel();
    }

    private void SaveGrahpicQualitySettingsData(GraphicQualitySettingsData graphicQualitySettingsData)
    {
        SaveSystem.SaveGrahpicQualitySettingsData(graphicQualitySettingsData);
    }

    private void LoadGraphicQualitySettingsData() {
        GraphicQualitySettingsData graphicQualitySettingsData = SaveSystem.LoadGrahpicQualitySettingsData();
        if (graphicQualitySettingsData != null)
        {
            Debug.Log($"그래픽 퀄리티 설정 정보 로드 성공!");

            QualitySettings.SetQualityLevel(graphicQualitySettingsData.qualityLevel, true);
            Debug.Log($"그래픽 퀄리티 설정 적용 성공!");
        }
        else
        {
            Debug.Log($"로드할 그래픽 퀄리티 설정 정보가 없습니다.");
        }
    }
}

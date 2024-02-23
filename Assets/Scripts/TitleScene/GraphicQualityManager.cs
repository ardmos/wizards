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
        Debug.Log($"�׷��� ����Ƽ ���� ���� ����!");

        // SaveData�� ����.
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
            Debug.Log($"�׷��� ����Ƽ ���� ���� �ε� ����!");

            QualitySettings.SetQualityLevel(graphicQualitySettingsData.qualityLevel, true);
            Debug.Log($"�׷��� ����Ƽ ���� ���� ����!");
        }
        else
        {
            Debug.Log($"�ε��� �׷��� ����Ƽ ���� ������ �����ϴ�.");
        }
    }
}

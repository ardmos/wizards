using UnityEngine;

/// <summary>
/// ���� �ڿ� ������ ���� �������� �������ִ� ��ũ��Ʈ
/// </summary>

public class TargetFPS : MonoBehaviour
{
    [SerializeField] private int target = 60;

    private void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = target;
    }
}

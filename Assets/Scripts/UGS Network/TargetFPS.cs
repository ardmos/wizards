using UnityEngine;

/// <summary>
/// 서버 자원 절약을 위해 프레임을 설정해주는 스크립트
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

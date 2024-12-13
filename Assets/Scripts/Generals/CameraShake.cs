using UnityEngine;
using Cinemachine;

public class CameraShake : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin;
    private float shakeTimer;
    private float shakeTimeTotal;
    private float startingIntensity;

    public void ShakeCamera(float intensity, float time)
    {
        cinemachineBasicMultiChannelPerlin =
            virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = intensity;

        startingIntensity = intensity;
        shakeTimeTotal = time;
        shakeTimer = time;
    }

    private void Update()
    {
        if (shakeTimer <= 0) return;

        shakeTimer -= Time.deltaTime;

        cinemachineBasicMultiChannelPerlin.m_AmplitudeGain =
            Mathf.Lerp(startingIntensity, 0f, 1 - (shakeTimer / shakeTimeTotal));        
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CineMachine ��� �������� �����ؾ���!
/// </summary>

public class CameraShake : MonoBehaviour
{
    private Animation anim;

    private void Start()
    {
        anim = GetComponent<Animation>();

    }

    public void ShakeCamera()
    {
        if (anim == null) return;

        anim.Play(anim.clip.name);
    }

}

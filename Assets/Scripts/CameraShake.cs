using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CineMachine 사용 버전으로 변경해야함!
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

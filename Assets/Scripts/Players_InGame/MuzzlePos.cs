using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// SpellManager에서 마법을 시전할 위치를 찾을 때 사용하는 스크립트 입니다.
/// </summary>
public class MuzzlePos : MonoBehaviour
{
    public Vector3 GetMuzzlePosition()
    {
        return transform.position;
    }
}

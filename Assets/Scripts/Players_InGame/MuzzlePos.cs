using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// SpellManager���� ������ ������ ��ġ�� ã�� �� ����ϴ� ��ũ��Ʈ �Դϴ�.
/// </summary>
public class MuzzlePos : MonoBehaviour
{
    public Vector3 GetMuzzlePosition()
    {
        return transform.position;
    }

    public Vector3 GetMuzzleLocalPosition()
    {
        return transform.localPosition;
    }
}

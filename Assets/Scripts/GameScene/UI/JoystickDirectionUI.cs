using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.OnScreen;
/// <summary>
/// 1. ���̽�ƽ �ڵ� ��ġ�� ���� ��Ŀ�� ���̶���Ʈ ȿ�� ���ֱ�
/// </summary>
public class JoystickDirectionUI : MonoBehaviour
{
    [SerializeField] private GameObject[] gameObjectFocus;
    [SerializeField] private GameObject gameObjectHandle;
    [SerializeField] private float focushHighlightResponsiveness;

    //[SerializeField] private OnScreenStick handle;

    // Start is called before the first frame update
    void Start()
    {
        //handle.OnPointerDown();
    }

    // Update is called once per frame
    void Update()
    {
        // �ڵ� �߾�, ��ü off
        foreach (GameObject go in gameObjectFocus)
        {
            go.SetActive(false);
        }

        Vector2 handleDir = gameObjectHandle.transform.localPosition - transform.localPosition;

        // �ڵ� ���, Focus_1
        if (handleDir.x > focushHighlightResponsiveness && handleDir.y > focushHighlightResponsiveness) gameObjectFocus[0].SetActive(true);
        // �ڵ� ����, Focus_2
        else if (handleDir.x > focushHighlightResponsiveness && handleDir.y < -focushHighlightResponsiveness) gameObjectFocus[1].SetActive(true);
        // �ڵ� ����, Focus_3
        else if (handleDir.x < -focushHighlightResponsiveness && handleDir.y < -focushHighlightResponsiveness) gameObjectFocus[2].SetActive(true);
        // �ڵ� �»�, Focus_4
        else if (handleDir.x < -focushHighlightResponsiveness && handleDir.y > focushHighlightResponsiveness) gameObjectFocus[3].SetActive(true);
        // �ڵ� ��, Focus_1 Focus_4
        else if (handleDir.y > focushHighlightResponsiveness)
        {
            gameObjectFocus[0].SetActive(true);
            gameObjectFocus[3].SetActive(true);
        }
        // �ڵ� ��, Focus_1 Focus_2
        else if (handleDir.x > focushHighlightResponsiveness)
        {
            gameObjectFocus[0].SetActive(true);
            gameObjectFocus[1].SetActive(true);
        }
        // �ڵ� ��, Focus_2 Focus_3
        else if (handleDir.y < -focushHighlightResponsiveness)
        {
            gameObjectFocus[1].SetActive(true);
            gameObjectFocus[2].SetActive(true);
        }
        // �ڵ� ��, Focus_3 Focus_4
        else if (handleDir.x < -focushHighlightResponsiveness)
        {
            gameObjectFocus[2].SetActive(true);
            gameObjectFocus[3].SetActive(true);
        }
    }
}

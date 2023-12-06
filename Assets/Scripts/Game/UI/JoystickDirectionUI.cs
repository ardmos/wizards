using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 1. ���̽�ƽ �ڵ� ��ġ�� ���� ��Ŀ�� ���̶���Ʈ ȿ�� ���ֱ�
/// </summary>
public class JoystickDirectionUI : MonoBehaviour
{
    [SerializeField] private GameObject[] gameObjectFocus;
    [SerializeField] private GameObject gameObjectHandle;
    [SerializeField] private float focushHighlightResponsiveness;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // �ڵ� �߾�, ��ü off
        foreach (GameObject go in gameObjectFocus)
        {
            go.SetActive(false);
        }

        Vector2 handlePos = gameObjectHandle.transform.localPosition;

        // �ڵ� ���, Focus_1
        if (handlePos.x > focushHighlightResponsiveness && handlePos.y > focushHighlightResponsiveness) gameObjectFocus[0].SetActive(true);
        // �ڵ� ����, Focus_2
        else if (handlePos.x > focushHighlightResponsiveness && handlePos.y < -focushHighlightResponsiveness) gameObjectFocus[1].SetActive(true);
        // �ڵ� ����, Focus_3
        else if (handlePos.x < -focushHighlightResponsiveness && handlePos.y < -focushHighlightResponsiveness) gameObjectFocus[2].SetActive(true);
        // �ڵ� �»�, Focus_4
        else if (handlePos.x < -focushHighlightResponsiveness && handlePos.y > focushHighlightResponsiveness) gameObjectFocus[3].SetActive(true);
        // �ڵ� ��, Focus_1 Focus_4
        else if (handlePos.y > focushHighlightResponsiveness)
        {
            gameObjectFocus[0].SetActive(true);
            gameObjectFocus[3].SetActive(true);
        }
        // �ڵ� ��, Focus_1 Focus_2
        else if (handlePos.x > focushHighlightResponsiveness)
        {
            gameObjectFocus[0].SetActive(true);
            gameObjectFocus[1].SetActive(true);
        }
        // �ڵ� ��, Focus_2 Focus_3
        else if (handlePos.y < -focushHighlightResponsiveness)
        {
            gameObjectFocus[1].SetActive(true);
            gameObjectFocus[2].SetActive(true);
        }
        // �ڵ� ��, Focus_3 Focus_4
        else if (handlePos.x < -focushHighlightResponsiveness)
        {
            gameObjectFocus[2].SetActive(true);
            gameObjectFocus[3].SetActive(true);
        }
    }
}

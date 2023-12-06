using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 1. 조이스틱 핸들 위치에 따라 포커스 하이라이트 효과 켜주기
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
        // 핸들 중앙, 전체 off
        foreach (GameObject go in gameObjectFocus)
        {
            go.SetActive(false);
        }

        Vector2 handlePos = gameObjectHandle.transform.localPosition;

        // 핸들 우상, Focus_1
        if (handlePos.x > focushHighlightResponsiveness && handlePos.y > focushHighlightResponsiveness) gameObjectFocus[0].SetActive(true);
        // 핸들 우하, Focus_2
        else if (handlePos.x > focushHighlightResponsiveness && handlePos.y < -focushHighlightResponsiveness) gameObjectFocus[1].SetActive(true);
        // 핸들 좌하, Focus_3
        else if (handlePos.x < -focushHighlightResponsiveness && handlePos.y < -focushHighlightResponsiveness) gameObjectFocus[2].SetActive(true);
        // 핸들 좌상, Focus_4
        else if (handlePos.x < -focushHighlightResponsiveness && handlePos.y > focushHighlightResponsiveness) gameObjectFocus[3].SetActive(true);
        // 핸들 상, Focus_1 Focus_4
        else if (handlePos.y > focushHighlightResponsiveness)
        {
            gameObjectFocus[0].SetActive(true);
            gameObjectFocus[3].SetActive(true);
        }
        // 핸들 우, Focus_1 Focus_2
        else if (handlePos.x > focushHighlightResponsiveness)
        {
            gameObjectFocus[0].SetActive(true);
            gameObjectFocus[1].SetActive(true);
        }
        // 핸들 하, Focus_2 Focus_3
        else if (handlePos.y < -focushHighlightResponsiveness)
        {
            gameObjectFocus[1].SetActive(true);
            gameObjectFocus[2].SetActive(true);
        }
        // 핸들 좌, Focus_3 Focus_4
        else if (handlePos.x < -focushHighlightResponsiveness)
        {
            gameObjectFocus[2].SetActive(true);
            gameObjectFocus[3].SetActive(true);
        }
    }
}

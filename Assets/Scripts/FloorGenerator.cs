using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 바닥 생성기
/// 씬이 시작되면 자동 생성. 
/// 원점(현 스크립트가 붙어있는 오브젝트) 를 전체 맵 사이즈의 좌상단으로 설정.
/// 한 칸 간의 간격 : 5m (한 타일이 10x10이기 때문)
/// </summary>
 
public class FloorGenerator : MonoBehaviour
{
    // 바닥으로 쓸 프리팹 
    [SerializeField] private Transform floorPref;

    // 좌우상하 몇 칸짜리 만들지.
    [SerializeField] private int mapSizeX, mapSizeZ;

    // 한 칸의 간격
    private float tileInterval = 5;

    // 오브젝트 Y 높이
    private float transformYValue = -0.5f;

    // 테스트용 벽 기둥
    [SerializeField] private GameObject walls, pillars, targets;

    // Start is called before the first frame update
    void Start()
    {
        // 벽, 기둥 생성
        walls.SetActive(true);
        pillars.SetActive(true);
        targets.SetActive(true);


        // 생성 가로(mapSizeX) 세로(mapSizeZ)
        for (int laneZ = mapSizeZ-1; laneZ >= 0; laneZ--)
        {
            for (int laneX = 0; laneX < mapSizeX; laneX++)
            {
                Transform generatedFloorTile = Instantiate(floorPref);
                generatedFloorTile.parent = transform;
                // 위치 잡기 
                generatedFloorTile.position = new Vector3(laneX * tileInterval, 0f, laneZ * tileInterval);
            }
        }

        // 현 부모 오브젝트를 이동시켜서 맵 타일 생성의 기준점 잡기(mapSIzeX,Z에 따라 좌상단으로 이동) 
        // 이동 계산식 나중에 손봐야하나???? 일단 고 
        transform.position = new Vector3(transform.position.x - (mapSizeX-1f) * tileInterval / 2, transformYValue, transform.position.z - (mapSizeZ - 1f) * tileInterval / 2);

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �ٴ� ������
/// ���� ���۵Ǹ� �ڵ� ����. 
/// ����(�� ��ũ��Ʈ�� �پ��ִ� ������Ʈ) �� ��ü �� �������� �»������ ����.
/// �� ĭ ���� ���� : 5m (�� Ÿ���� 10x10�̱� ����)
/// </summary>
 
public class FloorGenerator : MonoBehaviour
{
    // �ٴ����� �� ������ 
    [SerializeField] private Transform floorPref;

    // �¿���� �� ĭ¥�� ������.
    [SerializeField] private int mapSizeX, mapSizeZ;

    // �� ĭ�� ����
    private float tileInterval = 5;

    // ������Ʈ Y ����
    private float transformYValue = -0.5f;

    // �׽�Ʈ�� �� ���
    [SerializeField] private GameObject walls, pillars, targets;

    // Start is called before the first frame update
    void Start()
    {
        // ��, ��� ����
        walls.SetActive(true);
        pillars.SetActive(true);
        targets.SetActive(true);


        // ���� ����(mapSizeX) ����(mapSizeZ)
        for (int laneZ = mapSizeZ-1; laneZ >= 0; laneZ--)
        {
            for (int laneX = 0; laneX < mapSizeX; laneX++)
            {
                Transform generatedFloorTile = Instantiate(floorPref);
                generatedFloorTile.parent = transform;
                // ��ġ ��� 
                generatedFloorTile.position = new Vector3(laneX * tileInterval, 0f, laneZ * tileInterval);
            }
        }

        // �� �θ� ������Ʈ�� �̵����Ѽ� �� Ÿ�� ������ ������ ���(mapSIzeX,Z�� ���� �»������ �̵�) 
        // �̵� ���� ���߿� �պ����ϳ�???? �ϴ� �� 
        transform.position = new Vector3(transform.position.x - (mapSizeX-1f) * tileInterval / 2, transformYValue, transform.position.z - (mapSizeZ - 1f) * tileInterval / 2);

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

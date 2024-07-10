using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileGenerator : MonoBehaviour
{
    [Header("타일 프리팹들")]
    public GameObject tile_R_WE;
    public GameObject tile_R_NS;
    public GameObject tile_REnd_N;
    public GameObject tile_REnd_E;
    public GameObject tile_REnd_S;
    public GameObject tile_REnd_W;
    public GameObject tile_REdge_N;
    public GameObject tile_REdge_E;
    public GameObject tile_REdge_S;
    public GameObject tile_REdge_W;
    public GameObject tile_CR_WN;
    public GameObject tile_CR_NE;
    public GameObject tile_CR_WS;
    public GameObject tile_CR_SE;
    public GameObject tile_CR_WNE;
    public GameObject tile_CR_WSE;
    public GameObject tile_CR_NWS;
    public GameObject tile_CR_NES;

    public GameObject tile_G;
    public GameObject tile_GEdge_N;
    public GameObject tile_GEdge_E;
    public GameObject tile_GEdge_S;
    public GameObject tile_GEdge_W;

    public GameObject tile_G_NWCorner;
    public GameObject tile_G_NECorner;
    public GameObject tile_G_ESCorner;
    public GameObject tile_G_SWCorner;


    ///
    /// 배열에 따라 자동 배치
    ///

    [Header("타일 맵 설정")]
    public int mapWidth = 30;
    public int mapHeight = 30;
    public float tileSize = 2f;

    private int[,] tileMap;

    void Start()
    {
        GenerateRandomTileMap();
        PlaceTiles();
    }

    void GenerateRandomTileMap()
    {
        tileMap = new int[mapHeight, mapWidth];
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                tileMap[y, x] = Random.Range(0, 5); // 0: 잔디, 1-4: 도로
            }
        }
    }

    void PlaceTiles()
    {
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                Vector3 position = new Vector3(x * tileSize, 0, y * tileSize);
                GameObject tileToPlace = GetTilePrefab(x, y);
                if (tileToPlace != null)
                {
                    Instantiate(tileToPlace, position, Quaternion.identity, transform);
                }
            }
        }
    }

    GameObject GetTilePrefab(int x, int y)
    {
        int currentTile = tileMap[y, x];
        bool north = y < mapHeight - 1 && tileMap[y + 1, x] != 0;
        bool east = x < mapWidth - 1 && tileMap[y, x + 1] != 0;
        bool south = y > 0 && tileMap[y - 1, x] != 0;
        bool west = x > 0 && tileMap[y, x - 1] != 0;

        if (currentTile == 0) // 잔디 타일
        {
            if (north && !east && !south && !west) return tile_GEdge_N;
            if (!north && east && !south && !west) return tile_GEdge_E;
            if (!north && !east && south && !west) return tile_GEdge_S;
            if (!north && !east && !south && west) return tile_GEdge_W;
            if (north && east && !south && !west) return tile_G_NECorner;
            if (!north && east && south && !west) return tile_G_ESCorner;
            if (!north && !east && south && west) return tile_G_SWCorner;
            if (north && !east && !south && west) return tile_G_NWCorner;
            return tile_G;
        }
        else // 도로 타일
        {
            int connections = (north ? 1 : 0) + (east ? 1 : 0) + (south ? 1 : 0) + (west ? 1 : 0);

            switch (connections)
            {
                case 1:
                    if (north) return tile_REnd_N;
                    if (east) return tile_REnd_E;
                    if (south) return tile_REnd_S;
                    if (west) return tile_REnd_W;
                    break;
                case 2:
                    if (north && south) return tile_R_NS;
                    if (east && west) return tile_R_WE;
                    if (north && east) return tile_CR_NE;
                    if (north && west) return tile_CR_WN;
                    if (south && east) return tile_CR_SE;
                    if (south && west) return tile_CR_WS;
                    break;
                case 3:
                    if (!north) return tile_CR_WSE;
                    if (!east) return tile_CR_NWS;
                    if (!south) return tile_CR_WNE;
                    if (!west) return tile_CR_NES;
                    break;
                case 4:
                    return tile_CR_NES; // 모든 방향 연결
            }
        }

        return null; // 적절한 타일을 찾지 못한 경우
    }
}

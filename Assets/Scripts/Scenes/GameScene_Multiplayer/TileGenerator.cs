using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileGenerator : MonoBehaviour
{
    public enum TileType
    {
        Grass,
        Road_WE,
        Road_NS,
        RoadEnd_N,
        RoadEnd_E,
        RoadEnd_S,
        RoadEnd_W,
        RoadEdge_N,
        RoadEdge_E,
        RoadEdge_S,
        RoadEdge_W,
        Corner_WN,
        Corner_NE,
        Corner_WS,
        Corner_SE,
        Corner_WNE,
        Corner_WSE,
        Corner_NWS,
        Corner_NES,
        GrassEdge_N,
        GrassEdge_E,
        GrassEdge_S,
        GrassEdge_W,
        GrassCorner_NW,
        GrassCorner_NE,
        GrassCorner_ES,
        GrassCorner_SW
    }

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

    [Header("타일 맵 설정")]
    private int mapWidth = 30;
    private int mapHeight = 30;
    private float tileSize = 2f;

    private TileType[,] tileMap_WizardForest;

    // 에디터상에서 배치되고나서는 사용하지 않습니다. 맵 제작 단계에서만 사용
/*    void Start()
    {
        InitializeTileMap();
        PlaceTiles();
    }*/

    void InitializeTileMap()
    {
        /// 배열 타일 배치 방향 참고
        ///      S
        /// W       E
        ///      N

        tileMap_WizardForest = new TileType[,]
        {
            { TileType.GrassCorner_SW, TileType.GrassEdge_S, TileType.GrassEdge_S, TileType.GrassEdge_S, TileType.GrassEdge_S, TileType.GrassEdge_S, TileType.GrassEdge_S, TileType.GrassEdge_S, TileType.GrassEdge_S, TileType.GrassEdge_S, TileType.GrassEdge_S, TileType.GrassEdge_S, TileType.GrassEdge_S, TileType.GrassEdge_S, TileType.GrassEdge_S, TileType.GrassEdge_S, TileType.GrassEdge_S, TileType.GrassEdge_S, TileType.GrassEdge_S, TileType.GrassEdge_S, TileType.GrassEdge_S, TileType.GrassEdge_S, TileType.GrassEdge_S, TileType.GrassEdge_S, TileType.GrassEdge_S, TileType.GrassEdge_S, TileType.GrassEdge_S, TileType.GrassEdge_S, TileType.GrassEdge_S, TileType.GrassEdge_S, TileType.GrassCorner_ES},         
            { TileType.GrassEdge_W, TileType.Corner_NE, TileType.Road_WE, TileType.Road_WE, TileType.Road_WE, TileType.Road_WE, TileType.Road_WE, TileType.Road_WE, TileType.Road_WE, TileType.Road_WE, TileType.Road_WE, TileType.Road_WE, TileType.Road_WE, TileType.Road_WE, TileType.Road_WE, TileType.Corner_WNE, TileType.Road_WE, TileType.Road_WE, TileType.Road_WE, TileType.Road_WE, TileType.Road_WE, TileType.Road_WE, TileType.Road_WE, TileType.Road_WE, TileType.Road_WE, TileType.Road_WE, TileType.Road_WE, TileType.Road_WE, TileType.Road_WE, TileType.Corner_WN, TileType.GrassEdge_E},
            { TileType.GrassEdge_W, TileType.Road_NS, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Road_NS, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Road_NS, TileType.GrassEdge_E},
            { TileType.GrassEdge_W, TileType.Road_NS, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Road_NS, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Road_NS, TileType.GrassEdge_E},
            { TileType.GrassEdge_W, TileType.Road_NS, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Road_NS, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Road_NS, TileType.GrassEdge_E},
            { TileType.GrassEdge_W, TileType.Road_NS, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Road_NS, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Road_NS, TileType.GrassEdge_E},
            { TileType.GrassEdge_W, TileType.Road_NS, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Road_NS, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Road_NS, TileType.GrassEdge_E},
            { TileType.GrassEdge_W, TileType.Road_NS, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Road_NS, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Road_NS, TileType.GrassEdge_E},
            { TileType.GrassEdge_W, TileType.Road_NS, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Road_NS, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Road_NS, TileType.GrassEdge_E},
            { TileType.GrassEdge_W, TileType.Road_NS, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Road_NS, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Road_NS, TileType.GrassEdge_E},
            
            { TileType.GrassEdge_W, TileType.Road_NS, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Road_NS, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Road_NS, TileType.GrassEdge_E},           
            { TileType.GrassEdge_W, TileType.Road_NS, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Road_NS, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Road_NS, TileType.GrassEdge_E},
            { TileType.GrassEdge_W, TileType.Road_NS, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Road_NS, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Road_NS, TileType.GrassEdge_E},
            { TileType.GrassEdge_W, TileType.Road_NS, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Road_NS, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Road_NS, TileType.GrassEdge_E},
            { TileType.GrassEdge_W, TileType.Road_NS, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.RoadEdge_N, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Road_NS, TileType.GrassEdge_E},
            
            { TileType.GrassEdge_W, TileType.Corner_NES, TileType.Road_WE, TileType.Road_WE, TileType.Road_WE, TileType.Road_WE, TileType.Road_WE, TileType.Road_WE, TileType.Road_WE, TileType.Road_WE, TileType.Road_WE, TileType.Road_WE, TileType.Road_WE, TileType.Road_WE, TileType.Road_WE, TileType.Road_WE, TileType.Road_WE, TileType.Road_WE, TileType.Road_WE, TileType.Road_WE, TileType.Road_WE, TileType.Road_WE, TileType.Road_WE, TileType.Road_WE, TileType.Road_WE, TileType.Road_WE, TileType.Road_WE, TileType.Road_WE, TileType.Road_WE, TileType.Corner_NWS, TileType.GrassEdge_E},
            
            { TileType.GrassEdge_W, TileType.Road_NS, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.RoadEdge_S, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Road_NS, TileType.GrassEdge_E},
            { TileType.GrassEdge_W, TileType.Road_NS, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Road_NS, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Road_NS, TileType.GrassEdge_E},
            { TileType.GrassEdge_W, TileType.Road_NS, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Road_NS, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Road_NS, TileType.GrassEdge_E},
            { TileType.GrassEdge_W, TileType.Road_NS, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Road_NS, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Road_NS, TileType.GrassEdge_E},
            { TileType.GrassEdge_W, TileType.Road_NS, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Road_NS, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Road_NS, TileType.GrassEdge_E},
            
            { TileType.GrassEdge_W, TileType.Road_NS, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Road_NS, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Road_NS, TileType.GrassEdge_E},
            { TileType.GrassEdge_W, TileType.Road_NS, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Road_NS, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Road_NS, TileType.GrassEdge_E},
            { TileType.GrassEdge_W, TileType.Road_NS, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Road_NS, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Road_NS, TileType.GrassEdge_E},
            { TileType.GrassEdge_W, TileType.Road_NS, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Road_NS, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Road_NS, TileType.GrassEdge_E},
            { TileType.GrassEdge_W, TileType.Road_NS, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Road_NS, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Road_NS, TileType.GrassEdge_E},
            { TileType.GrassEdge_W, TileType.Road_NS, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Road_NS, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Road_NS, TileType.GrassEdge_E},
            { TileType.GrassEdge_W, TileType.Road_NS, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Road_NS, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Road_NS, TileType.GrassEdge_E},
            { TileType.GrassEdge_W, TileType.Road_NS, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Road_NS, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass, TileType.Road_NS, TileType.GrassEdge_E},
            { TileType.GrassEdge_W, TileType.Corner_SE, TileType.Road_WE, TileType.Road_WE, TileType.Road_WE, TileType.Road_WE, TileType.Road_WE, TileType.Road_WE, TileType.Road_WE, TileType.Road_WE, TileType.Road_WE, TileType.Road_WE, TileType.Road_WE, TileType.Road_WE, TileType.Road_WE, TileType.Corner_WSE, TileType.Road_WE, TileType.Road_WE, TileType.Road_WE, TileType.Road_WE, TileType.Road_WE, TileType.Road_WE, TileType.Road_WE, TileType.Road_WE, TileType.Road_WE, TileType.Road_WE, TileType.Road_WE, TileType.Road_WE, TileType.Road_WE, TileType.Corner_WS, TileType.GrassEdge_E},
            { TileType.GrassCorner_NW, TileType.GrassEdge_N, TileType.GrassEdge_N, TileType.GrassEdge_N, TileType.GrassEdge_N, TileType.GrassEdge_N, TileType.GrassEdge_N, TileType.GrassEdge_N, TileType.GrassEdge_N, TileType.GrassEdge_N, TileType.GrassEdge_N, TileType.GrassEdge_N, TileType.GrassEdge_N, TileType.GrassEdge_N, TileType.GrassEdge_N, TileType.GrassEdge_N, TileType.GrassEdge_N, TileType.GrassEdge_N, TileType.GrassEdge_N, TileType.GrassEdge_N, TileType.GrassEdge_N, TileType.GrassEdge_N, TileType.GrassEdge_N, TileType.GrassEdge_N, TileType.GrassEdge_N, TileType.GrassEdge_N, TileType.GrassEdge_N, TileType.GrassEdge_N, TileType.GrassEdge_N, TileType.GrassEdge_N, TileType.GrassCorner_NE}
        };

        // 배열의 크기를 자동으로 설정
        mapHeight = tileMap_WizardForest.GetLength(0);
        mapWidth = tileMap_WizardForest.GetLength(1);
    }

    void PlaceTiles()
    {
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                Vector3 position = new Vector3(x * tileSize, 0, y * tileSize);
                GameObject tileToPlace = GetTilePrefab(tileMap_WizardForest[y, x]);
                if (tileToPlace != null)
                {
                    //Instantiate(tileToPlace, position, Quaternion.identity, transform);
                    Instantiate(tileToPlace, position, tileToPlace.transform.rotation, transform);
                }
            }
        }
    }

    GameObject GetTilePrefab(TileType tileType)
    {
        switch (tileType)
        {
            case TileType.Grass: return tile_G;
            case TileType.Road_WE: return tile_R_WE;
            case TileType.Road_NS: return tile_R_NS;
            case TileType.RoadEnd_N: return tile_REnd_N;
            case TileType.RoadEnd_E: return tile_REnd_E;
            case TileType.RoadEnd_S: return tile_REnd_S;
            case TileType.RoadEnd_W: return tile_REnd_W;
            case TileType.RoadEdge_N: return tile_REdge_N;
            case TileType.RoadEdge_E: return tile_REdge_E;
            case TileType.RoadEdge_S: return tile_REdge_S;
            case TileType.RoadEdge_W: return tile_REdge_W;
            case TileType.Corner_WN: return tile_CR_WN;
            case TileType.Corner_NE: return tile_CR_NE;
            case TileType.Corner_WS: return tile_CR_WS;
            case TileType.Corner_SE: return tile_CR_SE;
            case TileType.Corner_WNE: return tile_CR_WNE;
            case TileType.Corner_WSE: return tile_CR_WSE;
            case TileType.Corner_NWS: return tile_CR_NWS;
            case TileType.Corner_NES: return tile_CR_NES;
            case TileType.GrassEdge_N: return tile_GEdge_N;
            case TileType.GrassEdge_E: return tile_GEdge_E;
            case TileType.GrassEdge_S: return tile_GEdge_S;
            case TileType.GrassEdge_W: return tile_GEdge_W;
            case TileType.GrassCorner_NW: return tile_G_NWCorner;
            case TileType.GrassCorner_NE: return tile_G_NECorner;
            case TileType.GrassCorner_ES: return tile_G_ESCorner;
            case TileType.GrassCorner_SW: return tile_G_SWCorner;
            default: return null;
        }
    }
}

using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public class TileData
{
    public Vector3Int position;
    public string tileType; // 타일의 이름 또는 ID
}

[System.Serializable]
public class TileMapData
{
    public List<TileData> tiles = new List<TileData>();
}

public class MapManager : MonoBehaviour
{
    public Tilemap targetTilemap; // 병합할 최종 타일맵
    public List<Tilemap> sourceTilemaps; // 10개의 타일맵을 담을 리스트
    public List<string> tilemapFilePaths; // 각 타일맵의 JSON 파일 경로 리스트
    List<int> mapList;
    List<int> selectedMaps;
    private Dictionary<string, TileBase> tileDictionary; // 타일 이름 또는 ID로 타일 참조
    

    void Start()
    {
        mapList = new List<int> {0,1, 2, 3, 4, 5, 6, 7, 8, 9};
        selectedMaps = GetRandomNumbers(mapList, 3);
        LoadTileAssets();
        MergeTilemaps();
    }

    // 타일 애셋을 미리 불러오는 함수
    void LoadTileAssets()
    {
        tileDictionary = new Dictionary<string, TileBase>();
        TileBase[] allTiles = Resources.LoadAll<TileBase>("Tiles"); // Resources/Tiles 폴더에 있는 타일들을 불러옴
        foreach (TileBase tile in allTiles)
        {
            tileDictionary[tile.name] = tile;
        }
    }
    void MergeTilemaps()
    {
        Tilemap tilemap1 = sourceTilemaps[selectedMaps[0]];      // 첫 번째 타일맵
        Tilemap tilemap2= sourceTilemaps[selectedMaps[1]];      // 두 번째 타일맵
        Tilemap tilemap3= sourceTilemaps[selectedMaps[2]];      // 세 번째 타일맵
        // 타일맵들의 크기 계산
        Vector3Int map1Size = tilemap1.size;
        Vector3Int map2Size = tilemap2.size;
        Vector3Int map3Size = tilemap3.size;

        // 타일을 이어 붙이기
        CopyTilemapToTarget(tilemap1, new Vector3Int(0, 0, 0));  // 첫 번째 타일맵은 (0, 0) 위치에
        CopyTilemapToTarget(tilemap2, new Vector3Int(map1Size.x, 0, 0));  // 두 번째 타일맵은 첫 번째 타일맵 뒤에
        CopyTilemapToTarget(tilemap3, new Vector3Int(map1Size.x + map2Size.x, 0, 0));  // 세 번째 타일맵은 두 번째 타일맵 뒤에
    }

    void CopyTilemapToTarget(Tilemap sourceTilemap, Vector3Int offset)
    {
        // sourceTilemap의 타일들을 타겟 타일맵에 복사
        BoundsInt bounds = sourceTilemap.cellBounds;

        foreach (var position in bounds.allPositionsWithin)
        {
            // 현재 위치의 타일을 가져와서 targetTilemap에 복사
            TileBase tile = sourceTilemap.GetTile(position);
            if (tile != null)
            {
                // 타겟 타일맵에 타일을 추가할 위치 계산
                Vector3Int targetPosition = position + offset;
                targetTilemap.SetTile(targetPosition, tile);
            }
        }
    }
    private List<int> GetRandomNumbers(List<int> originalList, int count)
    {
        List<int> tempList = new List<int>(originalList);
        List<int> resultList = new List<int>();

        for (int i = 0; i < count; i++)
        {
            int randomIndex = Random.Range(0, tempList.Count);
            resultList.Add(tempList[randomIndex]);
            tempList.RemoveAt(randomIndex);
        }

        return resultList;
    }
}

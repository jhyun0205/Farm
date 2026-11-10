using Farm.GameData;
using Farm.GameData.Crop;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.WSA;


namespace Farm.TileSystem.Test
{
    public class SeasonalTileChangeMockUp : MonoBehaviour
    {
        [SerializeField] private Season startSeason;

        TileBase tile;
        Tilemap tilemap;
        public void Test()
        {
            TileData tileData = new TileData();
            tile.GetTileData(Vector3Int.zero, tilemap, ref tileData);
            tileData.sprite = null; // 차후 실제 스프라이트 사용
        }
    }
}


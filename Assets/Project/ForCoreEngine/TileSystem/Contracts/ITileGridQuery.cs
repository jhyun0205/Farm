using UnityEngine;

namespace CoreEngine.TileSystem.Contracts
{
    /// <summary>
    /// 그리드 데이터를 변경하지 않고 조회만 수행하는 읽기 전용 인터페이스
    /// </summary>
    public interface ITileGridQuery
    {
        // 좌표 변환
        Vector2Int WorldToCell(Vector3 worldPos);
        Vector3 CellToWorldCenter(Vector2Int cellPos);

        // 겹쳐 있는 타일 중 레이어 인덱스가 가장 높은 최상단 타일 반환
        bool TryGetTopTile(Vector2Int cellPos, out TileSnapshot topTile);

        // 특정 레이어, 특정 TileID, 또는 특정 카테고리 마스크에 부합하는 타일이 있는지 반환
        bool TryGetTileAtLayer(Vector2Int cellPos, byte layerIndex, out TileSnapshot tile);
        bool TryGetTileById(Vector2Int cellPos, ulong targetTileId, out TileSnapshot matchedTile);
        bool TryGetTileByCategory(Vector2Int cellPos, int categoryMask, out TileSnapshot matchedTile);

        // 캐릭터 중심 셀 기준 정사각형(Chebyshev) 범위 내 여부 판정
        bool IsWithinSquareRange(Vector2Int centerCell, Vector2Int targetCell, int range);

        // 캐릭터가 걸어서 진입 가능한 타일인지 판정
        bool IsWalkable(Vector2Int cellPos);
    }
}
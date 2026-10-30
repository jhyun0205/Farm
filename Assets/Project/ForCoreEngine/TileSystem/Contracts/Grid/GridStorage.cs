using System;
using CoreEngine.TileSystem.Contracts;
using CoreEngine.TileSystem.Math; // GridRangeCalculator 참조용
using UnityEngine;

namespace CoreEngine.TileSystem.Grid
{
    /// <summary>
    /// 레이어별 1차원 평탄화 배열(Flat Array) 기반의 다중 레이어 타일 스토리지
    /// </summary>
    public sealed partial class GridStorage : ITileGridQuery
    {
        private readonly int _width;
        private readonly int _height;
        private readonly byte _layerCount;
        private readonly Vector2Int _origin;

        private readonly InternalTileCell[][] _layerCells;
        private readonly ITileDataProvider _dataProvider;

        public int Width => _width;
        public int Height => _height;
        public byte LayerCount => _layerCount;
        public Vector2Int Origin => _origin;

        public GridStorage(
            int width,
            int height,
            byte layerCount,
            Vector2Int origin,
            ITileDataProvider dataProvider)
        {
            if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
            if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));
            if (layerCount == 0) throw new ArgumentOutOfRangeException(nameof(layerCount));
            if (dataProvider == null) throw new ArgumentNullException(nameof(dataProvider));

            _width = width;
            _height = height;
            _layerCount = layerCount;
            _origin = origin;
            _dataProvider = dataProvider;

            // 오타 수정: width * height
            int cellCountPerLayer = width * height;
            _layerCells = new InternalTileCell[layerCount][];
            for (int i = 0; i < layerCount; i++)
            {
                _layerCells[i] = new InternalTileCell[cellCountPerLayer];
            }
        }

        #region ITileGridQuery - 좌표 변환 및 기본 판정

        public Vector2Int WorldToCell(Vector3 worldPos)
        {
            // 기본 1x1 유닛 타일 기준 (추후 UnityEngine.Grid와 연동하여 보정 가능)
            return new Vector2Int(Mathf.FloorToInt(worldPos.x), Mathf.FloorToInt(worldPos.y));
        }

        public Vector3 CellToWorldCenter(Vector2Int cellPos)
        {
            // 타일의 정중앙(0.5, 0.5) 좌표 반환
            return new Vector3(cellPos.x + 0.5f, cellPos.y + 0.5f, 0f);
        }

        public bool IsWithinSquareRange(Vector2Int centerCell, Vector2Int targetCell, int range)
        {
            // 1단계에서 구현한 수학 모듈 재사용
            return GridRangeCalculator.IsWithinSquareRange(centerCell, targetCell, range);
        }

        public bool IsWalkable(Vector2Int cellPos)
        {
            if (!IsInBounds(cellPos)) return false;

            // 추후 충돌체(장애물) CategoryMask를 받아 검사하는 로직으로 확장 가능
            // 현재는 맵 범위를 벗어나지 않으면 이동 가능한 것으로 기본 판정
            return true;
        }

        #endregion

        #region ITileGridQuery - 레이어 역순 탐색

        public bool IsInBounds(Vector2Int cellPos)
        {
            int localX = cellPos.x - _origin.x;
            int localY = cellPos.y - _origin.y;
            return (uint)localX < (uint)_width && (uint)localY < (uint)_height;
        }

        public bool IsValidLayer(byte layerIndex) => layerIndex < _layerCount;

        public bool TryGetTopTile(Vector2Int cellPos, out TileSnapshot topTile)
        {
            if (!TryGetFlatIndex(cellPos, out int flatIndex))
            {
                topTile = default;
                return false;
            }

            for (int layer = _layerCount - 1; layer >= 0; layer--)
            {
                ref readonly InternalTileCell cell = ref _layerCells[layer][flatIndex];
                if (!cell.IsEmpty)
                {
                    topTile = cell.ToSnapshot(cellPos, (byte)layer);
                    return true;
                }
            }

            topTile = default;
            return false;
        }

        // 이름 불일치 수정: TryGetTileByLayer -> TryGetTileAtLayer
        public bool TryGetTileAtLayer(Vector2Int cellPos, byte layerIndex, out TileSnapshot tile)
        {
            if (layerIndex >= _layerCount || !TryGetFlatIndex(cellPos, out int flatIndex))
            {
                tile = default;
                return false;
            }

            ref readonly InternalTileCell cell = ref _layerCells[layerIndex][flatIndex];
            if (!cell.IsEmpty)
            {
                tile = cell.ToSnapshot(cellPos, layerIndex);
                return true;
            }

            tile = default;
            return false;
        }

        // 이름 불일치 수정: ContainsTile -> TryGetTileById
        public bool TryGetTileById(Vector2Int cellPos, ulong targetTileId, out TileSnapshot matchedTile)
        {
            if (targetTileId == 0 || !TryGetFlatIndex(cellPos, out int flatIndex))
            {
                matchedTile = default;
                return false;
            }

            for (int layer = _layerCount - 1; layer >= 0; layer--)
            {
                ref readonly InternalTileCell cell = ref _layerCells[layer][flatIndex];
                if (cell.TileID == targetTileId)
                {
                    matchedTile = cell.ToSnapshot(cellPos, (byte)layer);
                    return true;
                }
            }

            matchedTile = default;
            return false;
        }

        public bool TryGetTileByCategory(Vector2Int cellPos, int categoryMask, out TileSnapshot matchedTile)
        {
            if (categoryMask == 0 || !TryGetFlatIndex(cellPos, out int flatIndex))
            {
                matchedTile = default;
                return false;
            }

            for (int layer = _layerCount - 1; layer >= 0; layer--)
            {
                ref readonly InternalTileCell cell = ref _layerCells[layer][flatIndex];
                if (!cell.IsEmpty && (cell.CategoryMask & categoryMask) != 0)
                {
                    matchedTile = cell.ToSnapshot(cellPos, (byte)layer);
                    return true;
                }
            }

            matchedTile = default;
            return false;
        }

        #endregion

        public void ClearAll()
        {
            for (int i = 0; i < _layerCount; i++)
            {
                Array.Clear(_layerCells[i], 0, _layerCells[i].Length);
            }
        }

        private bool TryGetFlatIndex(Vector2Int cellPos, out int flatIndex)
        {
            int localX = cellPos.x - _origin.x;
            int localY = cellPos.y - _origin.y;

            if ((uint)localX >= (uint)_width || (uint)localY >= (uint)_height)
            {
                flatIndex = -1;
                return false;
            }

            // 오타 수정: localY * _width + localX
            flatIndex = localY * _width + localX;
            return true;
        }
    }
}
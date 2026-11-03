using CoreEngine.EventBus;
using CoreEngine.TileSystem.Contracts;
using CoreEngine.TileSystem.Math;
using UnityEngine;

namespace CoreEngine.TileSystem.Grid
{
    public sealed partial class GridStorage : ITileGridMutator
    {
        private static readonly Vector2Int[] ClockwiseNeighborOffsets =
        {
            new Vector2Int(0, 1),   // North     (1 << 0 = 1)
            new Vector2Int(1, 1),   // NorthEast (1 << 1 = 2)
            new Vector2Int(1, 0),   // East      (1 << 2 = 4)
            new Vector2Int(1, -1),  // SouthEast (1 << 3 = 8)
            new Vector2Int(0, -1),  // South     (1 << 4 = 16)
            new Vector2Int(-1, -1), // SouthWest (1 << 5 = 32)
            new Vector2Int(-1, 0),  // West      (1 << 6 = 64)
            new Vector2Int(-1, 1)   // NorthWest (1 << 7 = 128)
        };

        private readonly TileChangedEvent[] _changeBuffer = new TileChangedEvent[9];

        public bool PlaceTileWithRules(Vector2Int cellPos, ulong tileId)
        {
            if (tileId == 0 || !TryGetFlatIndex(cellPos, out int centerFlatIndex))
            {
                return false;
            }

            if (!_dataProvider.TryGetTileCreationInfo(tileId, out TileCreationInfo info))
            {
                return false;
            }

            byte layerIndex = info.LayerIndex;
            if (layerIndex >= _layerCount)
            {
                return false;
            }

            InternalTileCell[] layerArray = _layerCells[layerIndex];
            ref InternalTileCell targetCell = ref layerArray[centerFlatIndex];

            // 1단계: 중복 설치 차단
            if (targetCell.TileID == tileId)
            {
                return false;
            }

            // 2단계: 데이터 선반영
            targetCell.TileID = info.TileID;
            targetCell.ConnectGroupID = info.ConnectGroupID;
            targetCell.CategoryMask = info.CategoryMask;
            targetCell.CurrentDurability = Mathf.Max(1, info.MaxDurability);
            targetCell.RuleType = info.RuleType;
            targetCell.MaskIndex = 0;

            // 3단계 & 4단계: 주변 3x3(1-Depth) 마스크 재산출 및 이벤트 발행
            Recalculate3x3AndPublish(cellPos, layerIndex, layerArray);
            return true;
        }

        public bool ApplyTileWork(Vector2Int cellPos, byte layerIndex, int workAmount, out bool isCompleted)
        {
            isCompleted = false;

            if (workAmount <= 0 || layerIndex >= _layerCount || !TryGetFlatIndex(cellPos, out int flatIndex))
            {
                return false;
            }

            InternalTileCell[] layerArray = _layerCells[layerIndex];
            ref InternalTileCell targetCell = ref layerArray[flatIndex];

            if (targetCell.IsEmpty)
            {
                return false;
            }

            targetCell.CurrentDurability -= workAmount;

            // 내구도가 아직 남아있으면 작업 진행 성공만 반환 (타일 형태 변화 없음)
            if (targetCell.CurrentDurability > 0)
            {
                return true;
            }

            // 내구도가 0 이하가 되어 파괴(작업 완료)된 경우: 셀 비우기 후 주변 3x3 마스크 갱신 및 이벤트 발행
            isCompleted = true;
            targetCell.Clear();
            Recalculate3x3AndPublish(cellPos, layerIndex, layerArray);
            return true;
        }

        public bool RemoveTileWithRules(Vector2Int cellPos, byte layerIndex)
        {
            if (layerIndex >= _layerCount || !TryGetFlatIndex(cellPos, out int centerFlatIndex))
            {
                return false;
            }

            InternalTileCell[] layerArray = _layerCells[layerIndex];
            ref InternalTileCell targetCell = ref layerArray[centerFlatIndex];

            if (targetCell.IsEmpty)
            {
                return false;
            }

            targetCell.Clear();
            Recalculate3x3AndPublish(cellPos, layerIndex, layerArray);
            return true;
        }

        private void Recalculate3x3AndPublish(Vector2Int centerPos, byte layerIndex, InternalTileCell[] layerArray)
        {
            int changedCount = 0;

            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    Vector2Int currentPos = new Vector2Int(centerPos.x + dx, centerPos.y + dy);
                    if (!TryGetFlatIndex(currentPos, out int currentFlatIndex))
                    {
                        continue;
                    }

                    bool isCenter = dx == 0 && dy == 0;
                    ref InternalTileCell currentCell = ref layerArray[currentFlatIndex];

                    if (currentCell.IsEmpty)
                    {
                        // 중심 타일이 파괴되어 비워진 경우 tileId = 0 이벤트 기록
                        if (isCenter)
                        {
                            _changeBuffer[changedCount++] = new TileChangedEvent(currentPos, layerIndex, 0UL, 0);
                        }
                        continue;
                    }

                    byte newMaskIndex = ComputeCellMaskIndex(currentPos, in currentCell, layerArray);
                    bool maskChanged = currentCell.MaskIndex != newMaskIndex;

                    if (maskChanged)
                    {
                        currentCell.MaskIndex = newMaskIndex;
                    }

                    if (isCenter || maskChanged)
                    {
                        _changeBuffer[changedCount++] = new TileChangedEvent(
                            currentPos,
                            layerIndex,
                            currentCell.TileID,
                            currentCell.MaskIndex);
                    }
                }
            }

            for (int i = 0; i < changedCount; i++)
            {
                PublishTileChanged(in _changeBuffer[i]);
            }
        }

        private void PublishTileChanged(in TileChangedEvent evt)
        {
            // CoreEngine.EventBus 발행 호출부 (사용 중인 EventBus API 메서드명에 맞춰 연결)
            EventBus<TileChangedEvent>.Publish(evt);
            // 구독은 EventBus<TileChangedEvent>.Subscribe(메서드);
        }

        private byte ComputeCellMaskIndex(
            Vector2Int cellPos,
            in InternalTileCell centerCell,
            InternalTileCell[] layerArray)
        {
            if (centerCell.RuleType == BitmaskRuleType.None)
            {
                return 0;
            }

            byte rawMask = 0;

            for (int i = 0; i < ClockwiseNeighborOffsets.Length; i++)
            {
                if (centerCell.RuleType == BitmaskRuleType.Cardinal4 && (i & 1) != 0)
                {
                    continue;
                }

                Vector2Int neighborPos = cellPos + ClockwiseNeighborOffsets[i];
                if (!TryGetFlatIndex(neighborPos, out int neighborFlatIndex))
                {
                    continue;
                }

                ref readonly InternalTileCell neighborCell = ref layerArray[neighborFlatIndex];
                if (AreTilesConnected(in centerCell, in neighborCell))
                {
                    rawMask |= (byte)(1 << i);
                }
            }

            return TileBitmaskCalculator.ComputeMaskIndex(centerCell.RuleType, rawMask);
        }

        private static bool AreTilesConnected(in InternalTileCell a, in InternalTileCell b)
        {
            if (b.IsEmpty)
            {
                return false;
            }

            if (a.TileID == b.TileID)
            {
                return true;
            }

            return a.ConnectGroupID != 0 && a.ConnectGroupID == b.ConnectGroupID;
        }
    }
}
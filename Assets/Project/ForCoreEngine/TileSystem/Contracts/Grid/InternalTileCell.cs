using CoreEngine.TileSystem.Contracts;
using UnityEngine;

namespace CoreEngine.TileSystem.Grid
{
    /// <summary>
    /// 그리드 스토리지 내부 배열에서 직접 관리하는 가변 셀 데이터
    /// </summary>
    internal struct InternalTileCell
    {
        public ulong TileID;
        public ulong ConnectGroupID;
        public int CategoryMask;
        public int CurrentDurability;
        public BitmaskRuleType RuleType;
        public byte MaskIndex;

        public bool IsEmpty => TileID == 0;

        public void Clear()
        {
            this = default;
        }

        public TileSnapshot ToSnapshot(Vector2Int cellPos, byte layerIndex)
        {
            return new TileSnapshot(
                cellPos,
                layerIndex,
                TileID,
                ConnectGroupID,
                CategoryMask,
                CurrentDurability,
                RuleType,
                MaskIndex);
        }
    }
}
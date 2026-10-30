using System;
using UnityEngine;

namespace CoreEngine.TileSystem.Contracts
{
    public readonly struct TileSnapshot : IEquatable<TileSnapshot>
    {
        public readonly Vector2Int CellPos;
        public readonly byte LayerIndex;
        public readonly ulong TileID;
        public readonly ulong ConnectGroupID;
        public readonly int CategoryMask;
        public readonly int RemainingDurability;
        public readonly BitmaskRuleType RuleType;
        public readonly byte MaskIndex;

        public bool IsValid => TileID > 0;

        public TileSnapshot(
            Vector2Int cellPos,
            byte layerIndex,
            ulong tileId,
            ulong connectGroupId,
            int categoryMask,
            int remainingDurability,
            BitmaskRuleType ruleType,
            byte maskIndex)
        {
            CellPos = cellPos;
            LayerIndex = layerIndex;
            TileID = tileId;
            ConnectGroupID = connectGroupId;
            CategoryMask = categoryMask;
            RemainingDurability = remainingDurability;
            RuleType = ruleType;
            MaskIndex = maskIndex;
        }

        public bool Equals(TileSnapshot other) =>
            CellPos == other.CellPos &&
            TileID == other.TileID &&
            LayerIndex == other.LayerIndex;

        public override bool Equals(object obj) =>
            obj is TileSnapshot other && Equals(other);

        public override int GetHashCode() =>
            HashCode.Combine(CellPos, TileID, LayerIndex);

        public static bool operator ==(TileSnapshot left, TileSnapshot right) => left.Equals(right);
        public static bool operator !=(TileSnapshot left, TileSnapshot right) => !left.Equals(right);
    }
}
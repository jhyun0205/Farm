using System;
using UnityEngine;

namespace CoreEngine.TileSystem.Contracts
{
    /// <summary>
    /// 타일 조회 결과 전달 및 록온 대상의 무결성 검증을 위한 불변(Immutable) 값 타입
    /// </summary>
    public readonly struct TileSnapshot : IEquatable<TileSnapshot>
    {
        public readonly Vector2Int CellPos;
        public readonly int TileID;
        public readonly byte LayerIndex;
        public readonly int CategoryMask;
        public readonly int ConnectGroupID;
        public readonly int RemainingDurability;

        public bool IsValid => TileID > 0;

        public TileSnapshot(
            Vector2Int cellPos,
            int tileId,
            byte layerIndex,
            int categoryMask,
            int connectGroupId,
            int remainingDurability)
        {
            CellPos = cellPos;
            TileID = tileId;
            LayerIndex = layerIndex;
            CategoryMask = categoryMask;
            ConnectGroupID = connectGroupId;
            RemainingDurability = remainingDurability;
        }

        // 기능 4 예외 방어: 좌표, 레이어, 타일 ID가 모두 일치해야 동일 타겟으로 인정
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
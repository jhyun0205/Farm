using CoreEngine.EventBus;
using UnityEngine;

namespace CoreEngine.TileSystem.Contracts
{
    /// <summary>
    /// 타일 상태 변경 이벤트: (변경된 셀 좌표, 레이어 인덱스, 새 TileID - 제거 시 0, 계산된 오토타일 마스크 인덱스)
    /// 외부 요인으로 타일이 파괴될 때 록온을 해제하거나 렌더러가 스프라이트를 갱신할 때 사용
    /// </summary>
    public struct TileChangedEvent : IEvent
    {
        public Vector2Int CellPos;
        public byte LayerIndex;
        public ulong TileId;
        public byte MaskIndex;

        public TileChangedEvent(Vector2Int cellPos, byte layerIndex, ulong tileId, byte maskIndex)
        {
            CellPos = cellPos;
            LayerIndex = layerIndex;
            TileId = tileId;
            MaskIndex = maskIndex;
        }
    }

    /// <summary>
    /// 그리드 타일 데이터를 변경(설치/작업/파괴)하고 변경 이벤트를 발행하는 인터페이스
    /// </summary>
    public interface ITileGridMutator
    {
        // 비트마스크 규칙에 맞춰 타일을 설치하고 주변 3x3 영역을 1-Depth로 갱신
        bool PlaceTileWithRules(Vector2Int cellPos, ulong tileId);

        // 타일에 작업(피해량/진행도) 적용 -> 내구도가 0 이하가 되어 작업이 끝났다면 isCompleted = true 반환
        bool ApplyTileWork(Vector2Int cellPos, byte layerIndex, int workAmount, out bool isCompleted);
    }
}
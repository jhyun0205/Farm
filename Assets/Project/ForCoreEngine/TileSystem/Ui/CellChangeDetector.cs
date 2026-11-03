using CoreEngine.TileSystem.Contracts;
using UnityEngine;

namespace CoreEngine.TileSystem.UI
{
    /// <summary>
    /// 마우스 커서 셀 좌표 또는 플레이어(기준점) 셀 좌표가 변경되었을 때만 감지하는 필터
    /// </summary>
    public sealed class CellChangeDetector
    {
        private readonly ITileGridQuery _gridQuery;
        private Vector2Int _currentCursorCell;
        private Vector2Int _currentActorCell;
        private bool _isInitialized;

        public Vector2Int CurrentCell => _currentCursorCell;
        public Vector2Int CurrentActorCell => _currentActorCell;
        public bool IsInitialized => _isInitialized;

        public CellChangeDetector(ITileGridQuery gridQuery)
        {
            _gridQuery = gridQuery;
            _isInitialized = false;
        }

        /// <summary>
        /// 마우스 월드 좌표만 단독으로 추적할 때 사용합니다.
        /// </summary>
        public bool TryDetectCellChange(Vector3 cursorWorldPos, out Vector2Int cursorCellPos)
        {
            return TryDetectCellChange(cursorWorldPos, _currentActorCell, out cursorCellPos);
        }

        /// <summary>
        /// 마우스 월드 좌표와 플레이어 셀 좌표를 동시에 추적합니다.
        /// 마우스가 멈춰 있어도 플레이어가 다른 셀로 이동하면 true를 반환하여 사거리 UI를 갱신합니다.
        /// </summary>
        public bool TryDetectCellChange(Vector3 cursorWorldPos, Vector2Int actorCellPos, out Vector2Int cursorCellPos)
        {
            cursorCellPos = _gridQuery.WorldToCell(cursorWorldPos);

            if (!_isInitialized || _currentCursorCell != cursorCellPos || _currentActorCell != actorCellPos)
            {
                _currentCursorCell = cursorCellPos;
                _currentActorCell = actorCellPos;
                _isInitialized = true;
                return true;
            }

            return false;
        }

        /// <summary>
        /// 장착 슬롯 변경이나 그리드 원점 이동 시 강제로 다음 프레임에 재판정되도록 캐시를 초기화합니다.
        /// </summary>
        public void Invalidate()
        {
            _isInitialized = false;
        }
    }
}
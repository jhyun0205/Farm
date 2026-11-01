using CoreEngine.TileSystem.Contracts;
using UnityEngine;

namespace CoreEngine.TileSystem.UI
{
    /// <summary>
    /// 마우스 포인터가 위치한 그리드 셀 좌표가 변경되었을 때만 감지하는 필터
    /// </summary>
    public sealed class CellChangeDetector
    {
        private readonly ITileGridQuery _gridQuery;
        private Vector2Int _currentCell;
        private bool _isInitialized;

        public Vector2Int CurrentCell => _currentCell;
        public bool IsInitialized => _isInitialized;

        public CellChangeDetector(ITileGridQuery gridQuery)
        {
            _gridQuery = gridQuery;
            _isInitialized = false;
        }

        /// <summary>
        /// 월드 좌표를 셀 좌표로 변환하고, 직전 셀에서 이동했을 때만 true를 반환합니다.
        /// </summary>
        public bool TryDetectCellChange(Vector3 worldPos, out Vector2Int newCellPos)
        {
            newCellPos = _gridQuery.WorldToCell(worldPos);

            if (!_isInitialized || _currentCell != newCellPos)
            {
                _currentCell = newCellPos;
                _isInitialized = true;
                return true;
            }

            return false;
        }

        /// <summary>
        /// 강제로 현재 셀 캐시를 초기화하여 다음 프레임에 무조건 갱신되도록 만듭니다.
        /// (장착 슬롯 변경이나 플레이어 이동 시 재판정이 필요할 때 호출)
        /// </summary>
        public void Invalidate()
        {
            _isInitialized = false;
        }
    }
}
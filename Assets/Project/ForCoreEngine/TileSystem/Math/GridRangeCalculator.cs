using UnityEngine;

namespace CoreEngine.TileSystem.Math
{
    /// <summary>
    /// 캐릭터 중심 정사각형(Chebyshev Distance) 사거리 판정 및 체커 Clamp, 기하학 거리 연산기
    /// </summary>
    public static class GridRangeCalculator
    {
        /// <summary>
        /// 대상 셀이 캐릭터 중심 정사각형 사거리(n) 내에 있는지 판정
        /// </summary>
        public static bool IsWithinSquareRange(Vector2Int centerCell, Vector2Int targetCell, int range)
        {
            int safeRange = Mathf.Max(0, range);
            int dx = Mathf.Abs(targetCell.x - centerCell.x);
            int dy = Mathf.Abs(targetCell.y - centerCell.y);
            return Mathf.Max(dx, dy) <= safeRange;
        }

        /// <summary>
        /// 마우스 셀 좌표가 사거리를 벗어났을 때, 사거리 경계선(n)에 타일 체커를 고정(Axis-Aligned Clamp)한 좌표를 반환
        /// </summary>
        public static Vector2Int GetClampedCheckerCell(Vector2Int centerCell, Vector2Int mouseCell, int range)
        {
            int safeRange = Mathf.Max(0, range);
            int dx = mouseCell.x - centerCell.x;
            int dy = mouseCell.y - centerCell.y;

            int clampedDx = Mathf.Clamp(dx, -safeRange, safeRange);
            int clampedDy = Mathf.Clamp(dy, -safeRange, safeRange);

            return new Vector2Int(centerCell.x + clampedDx, centerCell.y + clampedDy);
        }

        // 타겟 탐색(TileTargetFinder)에서 공용으로 사용할 순수 기하학 함수들

        public static int GetChebyshevDistance(Vector2Int a, Vector2Int b) =>
            Mathf.Max(Mathf.Abs(a.x - b.x), Mathf.Abs(a.y - b.y));

        public static int GetManhattanDistance(Vector2Int a, Vector2Int b) =>
            Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);

        public static int GetSqrEuclideanDistance(Vector2Int a, Vector2Int b)
        {
            int dx = a.x - b.x;
            int dy = a.y - b.y;
            return (dx * dx) + (dy * dy);
        }

        /// <summary>
        /// 중심(Center) 대비 대상(Target)이 북(0)~북서(7) 중 어느 8방향 옥탈트에 속하는지 반환 (시계방향 순서)
        /// </summary>
        public static int GetClockwiseOctantIndex(Vector2Int center, Vector2Int target)
        {
            int dx = target.x - center.x;
            int dy = target.y - center.y;
            if (dx == 0 && dy == 0) return 0;

            // 각도를 계산하여 0~7 인덱스로 매핑 (북=0 기준 시계방향)
            float angle = Mathf.Atan2(dx, dy) * Mathf.Rad2Deg; // 북쪽이 0도 기준이 되도록 Atan2(dx, dy) 사용
            if (angle < 0f) angle += 360f;

            int octant = Mathf.FloorToInt((angle + 22.5f) / 45f) & 7;
            return octant;
        }
    }
}
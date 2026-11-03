using UnityEngine;
using CoreEngine.TileSystem.Contracts;
using CoreEngine.TileSystem.Math;

namespace CoreEngine.TileSystem.Logic
{
    /// <summary>
    /// 연속 상호작용(기능 5)을 위해 직전 작업 타일 주변의 다음 타겟을 가중치 기반으로 탐색합니다.
    /// </summary>
    public class TileTargetFinder
    {
        private readonly ITileGridQuery _gridQuery;

        public TileTargetFinder(ITileGridQuery gridQuery)
        {
            _gridQuery = gridQuery;
        }

        /// <summary>
        /// 방금 작업이 끝난 타일(lastTargetCell) 주변 8칸 중 다음 목표를 찾습니다.
        /// </summary>
        /// <param name="lastTargetCell">방금 작업이 완료된 타일 좌표</param>
        /// <param name="playerCell">현재 플레이어 셀 좌표 (사거리 판정용)</param>
        /// <param name="playerFacingDir">플레이어의 현재 시선 방향</param>
        /// <param name="maxRange">장착 아이템의 최대 유효 사거리</param>
        /// <param name="categoryMask">탐색할 대상 타일의 카테고리 마스크</param>
        /// <param name="nextTarget">탐색된 다음 목표 타일 좌표</param>
        public bool TryFindNextTarget(
            Vector2Int lastTargetCell,
            Vector2Int playerCell,
            Vector2Int playerFacingDir,
            int maxRange,
            int categoryMask,
            out Vector2Int nextTarget)
        {
            nextTarget = Vector2Int.zero;
            if (_gridQuery == null || categoryMask == 0) return false;

            Vector2Int bestTarget = Vector2Int.zero;
            float bestScore = float.MinValue;
            bool found = false;

            Vector2 facingVector = new Vector2(playerFacingDir.x, playerFacingDir.y).normalized;
            if (facingVector == Vector2.zero) facingVector = Vector2.down; // 기본값 하단

            // 공통 방향 유틸리티 사용 (북쪽부터 시작하는 시계방향 8방향 오프셋 배열)
            for (int i = 0; i < TileDirections.Clockwise8.Length; i++)
            {
                Vector2Int candidateCell = lastTargetCell + TileDirections.Clockwise8[i];

                // 1. 플레이어 사거리 내에 존재하는가?
                if (!_gridQuery.IsWithinSquareRange(playerCell, candidateCell, maxRange))
                    continue;

                // 2. 대상 카테고리 마스크와 일치하는 타일이 존재하는가?
                // (TryGetTileByCategory는 레이어 역순 탐색으로 가장 위에 있는 해당 카테고리 타일을 반환함)
                if (!_gridQuery.TryGetTileByCategory(candidateCell, categoryMask, out _))
                    continue;

                // 3. 가중치 점수 계산 (시선 방향 내적 > 인덱스 순서)
                // - 시선 방향과 일치할수록(내적 1.0) 높은 점수
                // - 시계 방향 인덱스를 미세하게 빼주어 방향 점수가 같을 경우 시계방향을 우선함
                Vector2 dirToCandidate = new Vector2(candidateCell.x - playerCell.x, candidateCell.y - playerCell.y).normalized;
                float dotProduct = Vector2.Dot(facingVector, dirToCandidate);

                float score = dotProduct - (i * 0.001f);

                if (score > bestScore)
                {
                    bestScore = score;
                    bestTarget = candidateCell;
                    found = true;
                }
            }

            if (found)
            {
                nextTarget = bestTarget;
                return true;
            }

            return false;
        }
    }
}
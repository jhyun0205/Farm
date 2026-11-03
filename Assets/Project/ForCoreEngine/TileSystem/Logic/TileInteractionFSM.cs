using System;
using UnityEngine;
using CoreEngine.TileSystem.Contracts;

namespace CoreEngine.TileSystem.Logic
{
    public enum TileFSMState
    {
        Idle,
        MovingToTarget, // 목표 타일로 접근 이동 중 (사거리 밖일 때)
        Executing,      // 목표 타일 인접 후 실제 작업(애니메이션) 진행 중
        SearchingNext,  // 현재 타일 파괴 후 다음 타겟을 찾는 중
        LockedOn        // 다음 타겟을 찾고 자동 이동/실행 대기 상태
    }

    /// <summary>
    /// 타일 상호작용(파괴/설치/작업) 및 연속 탐색(기능 5) 흐름을 관리하는 유한 상태 머신
    /// </summary>
    public class TileInteractionFSM
    {
        private readonly ITileGridQuery _gridQuery;
        private readonly ITileGridMutator _gridMutator;
        private readonly IPlayerActor _playerActor;
        private readonly TileTargetFinder _targetFinder;

        private TileFSMState _currentState = TileFSMState.Idle;
        private Vector2Int _currentTargetCell;
        private int _currentCategoryMask;
        private int _currentMaxRange;
        private byte _currentLayerIndex;

        public TileFSMState CurrentState => _currentState;
        public Vector2Int CurrentTargetCell => _currentTargetCell;

        // FSM 상태가 바뀔 때 어댑터(CharacterActionController 등)에 알림
        public event Action<TileFSMState, Vector2Int> OnStateChanged;

        public TileInteractionFSM(
            ITileGridQuery gridQuery,
            ITileGridMutator gridMutator,
            IPlayerActor playerActor)
        {
            _gridQuery = gridQuery ?? throw new ArgumentNullException(nameof(gridQuery));
            _gridMutator = gridMutator ?? throw new ArgumentNullException(nameof(gridMutator));
            _playerActor = playerActor ?? throw new ArgumentNullException(nameof(playerActor));
            _targetFinder = new TileTargetFinder(_gridQuery);
        }

        private void ChangeState(TileFSMState newState)
        {
            if (_currentState == newState) return;
            _currentState = newState;
            OnStateChanged?.Invoke(_currentState, _currentTargetCell);
        }

        /// <summary>
        /// 사용자가 타일을 클릭하여 새로운 상호작용을 지시할 때 호출합니다.
        /// </summary>
        public void StartInteraction(Vector2Int targetCell, byte layerIndex, int categoryMask, int maxRange)
        {
            _currentTargetCell = targetCell;
            _currentLayerIndex = layerIndex;
            _currentCategoryMask = categoryMask;
            _currentMaxRange = maxRange;

            EvaluateAndMoveToTarget();
        }

        /// <summary>
        /// 외부 개입(수동 이동, 장비 스왑, 피격 등)으로 진행 중인 작업을 강제 취소합니다.
        /// </summary>
        public void CancelInteraction()
        {
            if (_currentState == TileFSMState.Idle) return;

            _playerActor.CancelAutoMovement();
            ChangeState(TileFSMState.Idle);
        }

        /// <summary>
        /// 캐릭터 애니메이션의 타격 프레임(Hit Event) 등에서 타일에 실제 작업을 적용할 때 호출합니다.
        /// </summary>
        public void ApplyWorkTick(int workAmount)
        {
            if (_currentState != TileFSMState.Executing) return;

            bool success = _gridMutator.ApplyTileWork(_currentTargetCell, _currentLayerIndex, workAmount, out bool isCompleted);

            if (!success)
            {
                // 대상 타일이 이미 파괴되었거나 유효하지 않음 -> 중단
                CancelInteraction();
                return;
            }

            if (isCompleted)
            {
                // 타일 작업(파괴)이 완료되었으므로 다음 타겟 탐색 진입
                ChangeState(TileFSMState.SearchingNext);
                SearchNextTarget();
            }
        }

        private void EvaluateAndMoveToTarget()
        {
            // 체비쇼프 거리 1칸 이내면 인접한 것으로 간주하여 즉시 실행
            if (_gridQuery.IsWithinSquareRange(_playerActor.CurrentCell, _currentTargetCell, 1))
            {
                ChangeState(TileFSMState.Executing);
            }
            else
            {
                ChangeState(TileFSMState.MovingToTarget);
                _playerActor.MoveToAdjacentCell(
                    _currentTargetCell,
                    onArrived: () => ChangeState(TileFSMState.Executing),
                    onFailed: CancelInteraction
                );
            }
        }

        private void SearchNextTarget()
        {
            bool found = _targetFinder.TryFindNextTarget(
                lastTargetCell: _currentTargetCell,
                playerCell: _playerActor.CurrentCell,
                playerFacingDir: _playerActor.FacingDirection,
                maxRange: _currentMaxRange,
                categoryMask: _currentCategoryMask,
                out Vector2Int nextTarget);

            if (found)
            {
                _currentTargetCell = nextTarget;
                ChangeState(TileFSMState.LockedOn);

                // 기능 5 연속 실행: 다음 타겟을 찾았으므로 자동으로 해당 타겟으로 접근/실행 루프 재개
                EvaluateAndMoveToTarget();
            }
            else
            {
                // 더 이상 캘 수 있는 인접 타일이 없으면 Idle로 복귀
                ChangeState(TileFSMState.Idle);
            }
        }
    }
}
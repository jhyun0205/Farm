using UnityEngine;
using UnityEngine.Tilemaps; // 타일맵 관련 기능을 위해 필수!
using UnityEngine.InputSystem;
using CoreEngine.Actor;
using CoreEngine;
using CoreEngine.Helpers;
using System;
using CoreEngine.EventBus;
using CoreEngine.Pool;
using CoreEngine.TileSystem.Contracts;
using CoreEngine.TileSystem.UI;
using Farm.Systems.TileContent;
using Farm.Character.Move;
using Farm.GameData.Tile;

namespace Farm.Character
{
    /// <summary>
    /// 캐릭터가 현재 타겟으로하는 타일 확인해주는 객체
    /// </summary>
    [System.Serializable]
    public class CharacterTileChecker : BaseActorFeature, ITick, IPlayerActor, ISpawnable
    {
        [Header("Tile System Dependencies")]
        [SerializeField] private TileCheckerViewBase _cursorView; // 타일을 표시할 view (tileMarker 대체)
        [SerializeField] private InventoryRangeBinder _rangeBinder = new();

        private ITileGridQuery _gridQuery;
        private CellChangeDetector _cellDetector;
        private CharacterMoveFeature _moveFeature;

        // 현재 사용 중인 타일맵 (targetTilemap 대체: 런타임에 동적 주입됨)
        private Vector3 tileMarkerPos; // 타일마커의 현재 위치

        // IPlayerActor 규격 구현
        public Vector2Int CurrentCell => _gridQuery != null ? _gridQuery.WorldToCell(Host.transform.position) : Vector2Int.zero;
        public Vector2Int FacingDirection { get; private set; } = Vector2Int.down;
        public bool IsMoving => _moveFeature != null && _moveFeature.isMove;

        // 자동 이동 관련 필드
        private bool _isAutoMoving;
        private Vector2Int _autoMoveTargetCell;
        private Action _onArrived;
        private Action _onFailed;

        protected override void OnInitialized()
        {
            base.OnInitialized();
            // Host는 BaseActorFeature에서 할당되므로 즉시 사용 가능
            Host.TryGetFeature(out _moveFeature);
        }

        // ISpawnable 인터페이스 구현 (풀링 생명주기에 맞춘 구독 관리)
        public void OnSpawn()
        {
            EventBus<ManualMoveInputDetectedEvent>.Subscribe(OnManualMoveInput);
        }

        public void OnDespawn()
        {
            EventBus<ManualMoveInputDetectedEvent>.Unsubscribe(OnManualMoveInput);
            _rangeBinder.Unbind();
            CancelAutoMovement(); // 오브젝트가 풀로 돌아갈 때 이동 상태 초기화
        }

        /// <summary>
        /// 외부(TileManager 등)에서 코어 타일 데이터 참조를 주입
        /// </summary>
        public void InjectTileSystem(ITileGridQuery gridQuery, TileDataProvider dataProvider)
        {
            _gridQuery = gridQuery;
            _cellDetector = new CellChangeDetector(_gridQuery);
            _rangeBinder.Initialize(Host, dataProvider);
            _rangeBinder.Bind();
        }

        public void Tick(float deltaTime)
        {
            UpdateFacingDirection();
            UpdateAutoMovement();
            TileCheck();
        }

        private void UpdateFacingDirection()
        {
            if (_moveFeature == null || !_moveFeature.isMove) return;

            Vector2 input = _moveFeature.inputMove;
            if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
                FacingDirection = input.x > 0 ? Vector2Int.right : Vector2Int.left;
            else
                FacingDirection = input.y > 0 ? Vector2Int.up : Vector2Int.down;
        }

        private void TileCheck()
        {
            Vector3 curPos = Host.transform.position;
            // TODO 캐릭터 정면 +0.5 만큼 위치 이동
            curPos.z = 0f;

            if (_gridQuery == null || _cursorView == null || _cellDetector == null || Mouse.current == null || Camera.main == null)
            {
                //LogHelper.LogWarning("targetTilemap 동적 관리 필요");
                return;
            }

            Vector2 screenPos = Mouse.current.position.ReadValue();
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(screenPos);

            // 월드 좌표를 타일맵의 '셀(칸) 좌표'로 변환
            if (_cellDetector.TryDetectCellChange(mouseWorldPos, CurrentCell, out Vector2Int cellPosition))
            {
                // 해당 셀에 타일이 있는지 확인 (선택 사항: 타일이 있는 곳만 하이라이트 하고 싶을 때)
                // ITileGridQuery의 IsWithinSquareRange를 사용하여 유효 사거리 검증
                bool inRange = _gridQuery.IsWithinSquareRange(CurrentCell, cellPosition, _rangeBinder.CurrentRange);
                TileCheckerState state = inRange ? TileCheckerState.ValidInRange : TileCheckerState.OutOfRange;

                // 셀 좌표의 중앙 월드 좌표를 가져와서 하이라이트 오브젝트 위치 수정
                tileMarkerPos = _gridQuery.CellToWorldCenter(cellPosition);
                _cursorView.UpdateChecker(cellPosition, tileMarkerPos, state);
            }
        }

        // 실제 객체의 위치는 플레이어에 의해 바뀔 수 있으니 저장된 속성을 사용
        public Vector3 GetTilePosition() => tileMarkerPos;

        #region IPlayerActor 자동 접근 이동 구현부

        public void MoveToAdjacentCell(Vector2Int targetCell, Action onArrived, Action onFailed)
        {
            if (_gridQuery == null || _moveFeature == null || !_moveFeature.canMove)
            {
                onFailed?.Invoke();
                return;
            }

            // ITileGridQuery.IsWithinSquareRange 활용 (1칸 거리 이내 확인)
            if (_gridQuery.IsWithinSquareRange(CurrentCell, targetCell, 1))
            {
                onArrived?.Invoke();
                return;
            }

            _isAutoMoving = true;
            _autoMoveTargetCell = targetCell;
            _onArrived = onArrived;
            _onFailed = onFailed;
        }

        public void CancelAutoMovement()
        {
            if (_isAutoMoving)
            {
                _isAutoMoving = false;
                _onFailed?.Invoke();
                _onArrived = null;
                _onFailed = null;
                if (_moveFeature != null) _moveFeature.Move(Vector2.zero, false);
            }
        }

        private void OnManualMoveInput(ManualMoveInputDetectedEvent evt)
        {
            CancelAutoMovement();
        }

        private void UpdateAutoMovement()
        {
            if (!_isAutoMoving || _gridQuery == null || _moveFeature == null) return;

            if (_gridQuery.IsWithinSquareRange(CurrentCell, _autoMoveTargetCell, 1))
            {
                _isAutoMoving = false;
                _moveFeature.Move(Vector2.zero, false);

                Action cb = _onArrived;
                _onArrived = null; _onFailed = null;
                cb?.Invoke();
            }
            else
            {
                Vector3 targetPos = _gridQuery.CellToWorldCenter(_autoMoveTargetCell);
                Vector2 dir = (targetPos - Host.transform.position).normalized;
                _moveFeature.Move(dir, false);
            }
        }

        #endregion
    }
}
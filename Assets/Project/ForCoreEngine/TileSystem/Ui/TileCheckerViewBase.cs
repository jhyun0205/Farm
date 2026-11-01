using CoreEngine.TileSystem.Contracts;
using UnityEngine;
using UnityEngine.UI;

namespace CoreEngine.TileSystem.UI
{
    /// <summary>
    /// SubCanvas 격리 및 CanvasGroup.alpha 제어를 통해 
    /// 상위 Canvas 리빌드와 SetActive 오버헤드를 제거한 타일 체커 뷰 베이스
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class TileCheckerViewBase : MonoBehaviour, ITileCheckerView
    {
        [Header("Base Checker Components")]
        [SerializeField] private Canvas _subCanvas;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Image _cursorImage;

        [Header("State Colors")]
        [SerializeField] private Color _validInRangeColor = new Color(1f, 1f, 1f, 0.85f);
        [SerializeField] private Color _outOfRangeColor = new Color(1f, 0.3f, 0.3f, 0.45f);
        [SerializeField] private Color _lockedOnColor = new Color(1f, 0.85f, 0.2f, 1f);

        private Transform _cachedTransform;
        private Vector2Int _currentCellPos;
        private TileCheckerState _currentState = TileCheckerState.Hidden;
        private bool _isVisible = true;
        private bool _hasPositionInitialized;

        protected virtual void Awake()
        {
            _cachedTransform = transform;

            if (_subCanvas == null) _subCanvas = GetComponent<Canvas>();
            if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();

            // 커서 UI가 마우스 레이캐스트를 가로채지 않도록 차단
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;

            SetVisible(false);
        }

        public void UpdateChecker(Vector2Int cellPos, Vector3 worldCenter, TileCheckerState state)
        {
            if (state == TileCheckerState.Hidden)
            {
                _currentState = TileCheckerState.Hidden;
                SetVisible(false);
                return;
            }

            SetVisible(true);

            // 셀 좌표나 월드 위치가 변경되었을 때만 트랜스폼 갱신
            if (!_hasPositionInitialized || _currentCellPos != cellPos || _cachedTransform.position != worldCenter)
            {
                _currentCellPos = cellPos;
                _cachedTransform.position = worldCenter;
                _hasPositionInitialized = true;
            }

            // 상태가 변경되었을 때만 그래픽 속성(색상 등) 갱신
            if (_currentState != state)
            {
                _currentState = state;
                OnApplyCheckerState(state);
            }
        }

        public void SetVisible(bool isVisible)
        {
            if (_isVisible == isVisible) return;

            _isVisible = isVisible;
            _canvasGroup.alpha = isVisible ? 1f : 0f;
        }

        protected virtual void OnApplyCheckerState(TileCheckerState state)
        {
            if (_cursorImage == null) return;

            switch (state)
            {
                case TileCheckerState.ValidInRange:
                    _cursorImage.color = _validInRangeColor;
                    break;
                case TileCheckerState.OutOfRange:
                    _cursorImage.color = _outOfRangeColor;
                    break;
                case TileCheckerState.LockedOn:
                    _cursorImage.color = _lockedOnColor;
                    break;
            }
        }
    }
}
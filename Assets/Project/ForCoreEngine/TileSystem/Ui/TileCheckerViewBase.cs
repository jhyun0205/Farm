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
        private Vector3 _currentWorldCenter;
        private TileCheckerState _currentState = TileCheckerState.Hidden;
        private bool _isVisible = true;
        private bool _hasPositionInitialized;

        public Canvas SubCanvas => _subCanvas;
        public Vector2Int CurrentCellPos => _currentCellPos;
        public TileCheckerState CurrentState => _currentState;

        protected virtual void Awake()
        {
            _cachedTransform = transform;

            if (_subCanvas == null) _subCanvas = GetComponent<Canvas>();
            if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();

            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;

            if (_cursorImage != null)
            {
                _cursorImage.raycastTarget = false;
            }

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

            // C++ Transform.position 역조회 없이 순수 C# 캐시 값으로만 비교
            if (!_hasPositionInitialized || _currentCellPos != cellPos || _currentWorldCenter != worldCenter)
            {
                _currentCellPos = cellPos;
                _currentWorldCenter = worldCenter;
                _cachedTransform.position = worldCenter;
                _hasPositionInitialized = true;
            }

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
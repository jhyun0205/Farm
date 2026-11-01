using CoreEngine.EventBus;
using CoreEngine.TileSystem.Contracts;
using CoreEngine.TileSystem.Grid;
using CoreEngine.TileSystem.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

namespace Farm.Systems.TileContent.Tests
{
    public class MockTileGridTestRunner : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Tilemap _tilemap;
        [SerializeField] private Camera _mainCamera;
        [SerializeField] private TileCheckerViewBase _checkerView;

        [Header("Grid Dimensions")]
        [SerializeField] private int _gridWidth = 100;
        [SerializeField] private int _gridHeight = 100;

        [Header("Mock Data Settings")]
        [SerializeField] private ulong _mockTileId = 999;
        [SerializeField] private BitmaskRuleType _ruleType = BitmaskRuleType.Blob8;
        [SerializeField] private Sprite[] _mockSprites;

        private GridStorage _gridStorage;
        private MockTileDataProvider _mockProvider;
        private CellChangeDetector _cellDetector;

        private void Start()
        {
            if (_mainCamera == null) _mainCamera = Camera.main;

            _mockProvider = new MockTileDataProvider(_mockTileId, _ruleType, _mockSprites);
            Vector2Int gridOrigin = new Vector2Int(-_gridWidth / 2, -_gridHeight / 2);

            Vector3 worldOrigin = _tilemap != null ? _tilemap.transform.position : Vector3.zero;
            Vector2 cellSize = (_tilemap != null && _tilemap.layoutGrid != null)
                ? (Vector2)_tilemap.layoutGrid.cellSize
                : Vector2.one;

            _gridStorage = new GridStorage(
                _gridWidth,
                _gridHeight,
                1,
                gridOrigin,
                _mockProvider,
                worldOrigin,
                cellSize);

            _cellDetector = new CellChangeDetector(_gridStorage);

            EventBus<TileChangedEvent>.Subscribe(OnTileChanged);
        }

        private void OnDestroy()
        {
            EventBus<TileChangedEvent>.Unsubscribe(OnTileChanged);
        }

        private void Update()
        {
            if (_gridStorage == null || Mouse.current == null) return;

            if (_tilemap != null && _gridStorage.WorldOrigin != _tilemap.transform.position)
            {
                _gridStorage.SetWorldOrigin(_tilemap.transform.position);
                _cellDetector.Invalidate();
            }

            Vector2 screenPos = Mouse.current.position.ReadValue();
            Vector3 mouseWorldPos = _mainCamera.ScreenToWorldPoint(screenPos);

            if (_cellDetector.TryDetectCellChange(mouseWorldPos, out Vector2Int currentCell))
            {
                UpdateCheckerCursor(currentCell);
            }

            Vector2Int activeCell = _cellDetector.CurrentCell;

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                _gridStorage.PlaceTileWithRules(activeCell, _mockTileId);
            }
            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                _gridStorage.RemoveTileWithRules(activeCell, 0);
            }
        }

        private void UpdateCheckerCursor(Vector2Int cellPos)
        {
            if (_checkerView == null) return;

            if (!_gridStorage.IsInBounds(cellPos))
            {
                _checkerView.UpdateChecker(cellPos, Vector3.zero, TileCheckerState.Hidden);
                return;
            }

            Vector3 cellCenterWorld = _gridStorage.CellToWorldCenter(cellPos);
            _checkerView.UpdateChecker(cellPos, cellCenterWorld, TileCheckerState.ValidInRange);
        }

        private void OnTileChanged(TileChangedEvent evt)
        {
            Vector3Int pos = new Vector3Int(evt.CellPos.x, evt.CellPos.y, 0);

            if (evt.TileId == 0)
            {
                _tilemap.SetTile(pos, null);
                return;
            }

            Sprite sprite = _mockProvider.GetSprite(evt.MaskIndex);
            if (sprite != null)
            {
                Tile tile = ScriptableObject.CreateInstance<Tile>();
                tile.sprite = sprite;
                _tilemap.SetTile(pos, tile);
            }
        }
    }
}
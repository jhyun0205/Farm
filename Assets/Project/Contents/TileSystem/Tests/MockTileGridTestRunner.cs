using CoreEngine.EventBus;
using CoreEngine.TileSystem.Contracts;
using CoreEngine.TileSystem.Grid;
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

        [Header("Grid Dimensions")]
        [SerializeField] private int _gridWidth = 100;
        [SerializeField] private int _gridHeight = 100;

        [Header("Mock Data Settings")]
        [SerializeField] private ulong _mockTileId = 999;
        [SerializeField] private BitmaskRuleType _ruleType = BitmaskRuleType.Blob8;

        [Tooltip("오토타일 스프라이트 47개 (없으면 1개만 넣어도 0번 인덱스로 폴백됨)")]
        [SerializeField] private Sprite[] _mockSprites;

        private GridStorage _gridStorage;
        private MockTileDataProvider _mockProvider;

        private void Start()
        {
            if (_mainCamera == null) _mainCamera = Camera.main;

            _mockProvider = new MockTileDataProvider(_mockTileId, _ruleType, _mockSprites);

            // (0, 0)을 중심으로 사방(음수 좌표 포함)에 타일을 깔 수 있도록 원점을 (-Width/2, -Height/2)로 설정
            Vector2Int gridOrigin = new Vector2Int(-_gridWidth / 2, -_gridHeight / 2);

            // Tilemap의 월드 위치 및 상위 Grid의 셀 크기 추출
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

            EventBus<TileChangedEvent>.Subscribe(OnTileChanged);

            Debug.Log($"[Mock Test] 준비 완료! 유효 셀 범위: {gridOrigin} ~ ({gridOrigin.x + _gridWidth - 1}, {gridOrigin.y + _gridHeight - 1})");
        }

        private void OnDestroy()
        {
            EventBus<TileChangedEvent>.Unsubscribe(OnTileChanged);
        }

        private void Update()
        {
            if (_gridStorage == null || Mouse.current == null) return;

            // 테스트 중 씬 뷰에서 Tilemap을 드래그해 이동시켜도 즉시 좌표계가 일치하도록 갱신
            if (_tilemap != null)
            {
                _gridStorage.SetWorldOrigin(_tilemap.transform.position);
            }

            Vector2 screenPos = Mouse.current.position.ReadValue();
            Vector3 mouseWorldPos = _mainCamera.ScreenToWorldPoint(screenPos);
            Vector2Int cellPos = _gridStorage.WorldToCell(mouseWorldPos);

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                _gridStorage.PlaceTileWithRules(cellPos, _mockTileId);
            }
            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                _gridStorage.RemoveTileWithRules(cellPos, 0);
            }
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
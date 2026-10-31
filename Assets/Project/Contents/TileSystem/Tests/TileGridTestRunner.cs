using CoreEngine.TileSystem.Grid;
using Farm.GameData.Tile;
using UnityEngine;
using UnityEngine.InputSystem; // 새로운 Input System 네임스페이스

namespace Farm.Systems.TileContent.Tests
{
    public class TileGridTestRunner : MonoBehaviour
    {
        [Header("Rendering")]
        [SerializeField] private TileGridRenderer _renderer;
        [SerializeField] private Camera _mainCamera;

        [Header("Test Settings")]
        [SerializeField] private ulong _testTileHashedPK;
        [SerializeField] private byte _testLayerIndex = 0;

        private TileDataProvider _dataRegistry;
        private GridStorage _gridStorage;

        private void Start()
        {
            if (_mainCamera == null) _mainCamera = Camera.main;

            // 1. 데이터 레지스트리 생성 (Initialize 호출 불필요)
            _dataRegistry = new TileDataProvider();

            // 2. 렌더러 연동
            _renderer.Initialize(_dataRegistry);

            // 3. 다중 레이어 스토리지 생성 (가로 100, 세로 100, 3개 층, 원점 0,0)
            _gridStorage = new GridStorage(100, 100, 3, Vector2Int.zero, _dataRegistry);

            Debug.Log("[TestRunner] Tile System 초기화 완료. 좌클릭: 설치 / 우클릭: 제거");
        }

        private void Update()
        {
            if (_gridStorage == null || Mouse.current == null) return;

            // 새로운 Input System의 마우스 좌표 스크린 -> 월드 변환
            Vector2 screenPos = Mouse.current.position.ReadValue();
            Vector2 mousePos = _mainCamera.ScreenToWorldPoint(screenPos);
            Vector2Int cellPos = _gridStorage.WorldToCell(mousePos);

            // 좌클릭 (wasPressedThisFrame = GetMouseButtonDown과 동일)
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                if (_gridStorage.PlaceTileWithRules(cellPos, _testTileHashedPK))
                {
                    Debug.Log($"[TestRunner] {cellPos} 타일 설치됨");
                }
            }

            // 우클릭
            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                if (_gridStorage.RemoveTileWithRules(cellPos, _testLayerIndex))
                {
                    Debug.Log($"[TestRunner] {cellPos} 타일 제거됨");
                }
            }
        }
    }
}
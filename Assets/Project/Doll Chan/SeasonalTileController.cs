using CoreEngine.GameData;
using Farm.GameData;
using Farm.GameData.LevelDesign;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Farm.TileSystem
{
    using UnityTile = UnityEngine.Tilemaps.Tile;

    public class SeasonalTileController : MonoBehaviour
    {
        private readonly struct SeasonalTileBinding
        {
            public readonly UnityTile RuntimeTile;
            public readonly SeasonalTileRecord Record;

            public SeasonalTileBinding(UnityTile runtimeTile, SeasonalTileRecord record)
            {
                RuntimeTile = runtimeTile;
                Record = record;
            }
        }

        [Header("Target Tilemaps (비워두면 씬 전체 자동 탐색)")]
        [SerializeField] private Tilemap[] targetTilemaps;

        private readonly TableHandler<SeasonalTileTable, SeasonalTileRecord> seasonalTileTableHandler = new();

        // 원본 에셋 TileBase -> 런타임 인스턴스 매핑 (동적 타일 배치 및 Swap용)
        private readonly Dictionary<TileBase, UnityTile> assetToRuntimeTileMap = new();

        // 런타임 타일과 테이블 레코드 바인딩 목록
        private readonly List<SeasonalTileBinding> seasonalBindings = new();

        // 실제로 계절 타일을 포함하고 있어 리프레시가 필요한 타일맵 목록
        private readonly HashSet<Tilemap> affectedTilemaps = new();

        public Season CurrentSeason { get; private set; }
        public bool IsInitialized { get; private set; }

        public void Initialize(Season initialSeason)
        {
            if (IsInitialized) return;

            if (targetTilemaps == null || targetTilemaps.Length == 0)
            {
                targetTilemaps = FindObjectsByType<Tilemap>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            }

            BuildRuntimeTilesFromTable();
            BindTilemaps(targetTilemaps);

            IsInitialized = true;
            ChangeSeason(initialSeason, forceRefresh: true);
        }

        private void BuildRuntimeTilesFromTable()
        {
            // TODO: 프레임워크에 읽기전용 배열 반환 열어놓고 TableRouter.GetTable<SeasonalTileTable>()로 교체
            var allRecords = seasonalTileTableHandler.GetRecords((x) => true);

            foreach (var record in allRecords)
            {
                TileBase originalAsset = record.TileBaseId.Get();
                if (originalAsset == null || assetToRuntimeTileMap.ContainsKey(originalAsset))
                {
                    continue;
                }

                UnityTile runtimeTile = CreateRuntimeTileFrom(originalAsset);
                assetToRuntimeTileMap.Add(originalAsset, runtimeTile);
                seasonalBindings.Add(new SeasonalTileBinding(runtimeTile, record));
            }
        }

        private static UnityTile CreateRuntimeTileFrom(TileBase sourceAsset)
        {
            var runtimeTile = ScriptableObject.CreateInstance<UnityTile>();
            runtimeTile.name = $"{sourceAsset.name}_Runtime";

            if (sourceAsset is UnityTile sourceTile)
            {
                runtimeTile.sprite = sourceTile.sprite;
                runtimeTile.color = sourceTile.color;
                runtimeTile.transform = sourceTile.transform;
                runtimeTile.flags = sourceTile.flags;
                runtimeTile.colliderType = sourceTile.colliderType;
            }

            return runtimeTile;
        }

        /// <summary>
        /// 대상 타일맵들에서 계절 변환 대상 타일이 존재하는 경우에만 런타임 타일로 교체하고 관리 대상에 등록합니다.
        /// </summary>
        public void BindTilemaps(IEnumerable<Tilemap> tilemaps)
        {
            foreach (var tilemap in tilemaps)
            {
                if (tilemap == null) continue;

                bool hasSeasonalTile = false;

                foreach (var kvp in assetToRuntimeTileMap)
                {
                    TileBase originalAsset = kvp.Key;
                    UnityTile runtimeTile = kvp.Value;

                    // GetTilesBlock 배열 할당 없이 C++ 네이티브 레벨에서 타일 포함 여부 확인
                    if (tilemap.ContainsTile(originalAsset))
                    {
                        tilemap.SwapTile(originalAsset, runtimeTile);
                        hasSeasonalTile = true;
                    }
                }

                if (hasSeasonalTile)
                {
                    affectedTilemaps.Add(tilemap);
                }
            }
        }

        /// <summary>
        /// 지정한 계절로 모든 런타임 계절 타일의 스프라이트를 변경하고 관련 타일맵만 리프레시합니다.
        /// </summary>
        public void ChangeSeason(Season newSeason, bool forceRefresh = false)
        {
            if (!IsInitialized)
            {
                Debug.LogWarning("[SeasonalTileController] 초기화되기 전에 ChangeSeason이 호출되었습니다.");
                return;
            }

            if (!forceRefresh && CurrentSeason == newSeason)
            {
                return;
            }

            CurrentSeason = newSeason;

            foreach (var binding in seasonalBindings)
            {
                Sprite seasonalSprite = GetSpriteBySeason(binding.Record, newSeason);
                if (seasonalSprite != null)
                {
                    binding.RuntimeTile.sprite = seasonalSprite;
                }
            }

            RefreshAffectedTilemaps();
        }

        /// <summary>
        /// 런타임에 코드로 SetTile을 호출할 때 원본 에셋 대신 현재 계절이 동기화되는 런타임 타일을 반환합니다.
        /// (계절 타일이 아니면 원본 그대로 반환)
        /// </summary>
        public TileBase GetMappedTile(TileBase originalAsset)
        {
            if (originalAsset != null && assetToRuntimeTileMap.TryGetValue(originalAsset, out UnityTile runtimeTile))
            {
                return runtimeTile;
            }
            return originalAsset;
        }

        private static Sprite GetSpriteBySeason(SeasonalTileRecord record, Season season)
        {
            return season switch
            {
                Season.Spring => record.SpringSpriteId.Get(),
                Season.Summer => record.SummerSpriteId.Get(),
                Season.Fall => record.FallSpriteId.Get(),
                Season.Winter => record.WinterSpriteId.Get(),
                _ => null
            };
        }

        private void RefreshAffectedTilemaps()
        {
            foreach (var tilemap in affectedTilemaps)
            {
                if (tilemap != null)
                {
                    tilemap.RefreshAllTiles();
                }
            }
        }

        private void OnDestroy()
        {
            // 런타임에 생성한 ScriptableObject 인스턴스 메모리 해제
            foreach (var runtimeTile in assetToRuntimeTileMap.Values)
            {
                if (runtimeTile != null)
                {
                    Destroy(runtimeTile);
                }
            }

            assetToRuntimeTileMap.Clear();
            seasonalBindings.Clear();
            affectedTilemaps.Clear();
        }
    }
}
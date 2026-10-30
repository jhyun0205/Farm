using System.Collections.Generic;
using CoreEngine.GameData;
using CoreEngine.TileSystem.Contracts;
using UnityEngine;

namespace Farm.GameData
{
    public sealed class TileDataRegistry : ITileDataProvider
    {
        private const int MAX_BLOB8_SPRITE_COUNT = 47;

        private readonly TableHandler<TileMetaTable, TileMetaRecord> _tileMetaHandler = new();
        private readonly TableHandler<ItemRangeTable, ItemRangeRecord> _itemRangeHandler = new();
        private readonly TableHandler<TileRuleSpriteTable, TileRuleSpriteRecord> _spriteRuleHandler = new();

        private readonly Dictionary<ulong, Sprite[]> _resolvedSpriteCache;

        public TileDataRegistry(int initialTileCapacity = 64)
        {
            _resolvedSpriteCache = new Dictionary<ulong, Sprite[]>(initialTileCapacity);
        }

        public bool TryGetTileCreationInfo(ulong tileKey, out TileCreationInfo info)
        {
            if (_tileMetaHandler.TryGetRecord(tileKey, out TileMetaRecord record))
            {
                info = record.ToCreationInfo();
                return true;
            }

            info = default;
            return false;
        }

        public bool TryGetTileMeta(ulong tileKey, out TileMetaRecord record) =>
            _tileMetaHandler.TryGetRecord(tileKey, out record);

        public bool TryGetItemRange(ulong itemKey, out ItemRangeRecord record) =>
            _itemRangeHandler.TryGetRecord(itemKey, out record);

        public Sprite GetTileSprite(ulong tileKey, byte maskIndex)
        {
            if (!TryGetOrBuildSpriteArray(tileKey, out Sprite[] sprites) || sprites.Length == 0)
            {
                return null;
            }

            if (maskIndex >= sprites.Length || sprites[maskIndex] == null)
            {
                return sprites[0];
            }

            return sprites[maskIndex];
        }

        public void ClearSpriteCache() => _resolvedSpriteCache.Clear();

        private bool TryGetOrBuildSpriteArray(ulong tileKey, out Sprite[] sprites)
        {
            if (_resolvedSpriteCache.TryGetValue(tileKey, out sprites))
            {
                return sprites != null;
            }

            if (!_spriteRuleHandler.TryGetReferencingRecords(
                    TileRuleSpriteTable.REF_INDEX_TILE_FK,
                    tileKey,
                    out IReadOnlyList<TileRuleSpriteRecord> records))
            {
                _resolvedSpriteCache[tileKey] = null;
                sprites = null;
                return false;
            }

            sprites = new Sprite[MAX_BLOB8_SPRITE_COUNT];
            for (int i = 0; i < records.Count; i++)
            {
                TileRuleSpriteRecord rec = records[i];
                if (rec.MaskIndex < MAX_BLOB8_SPRITE_COUNT)
                {
                    sprites[rec.MaskIndex] = rec.TileSprite;
                }
            }

            _resolvedSpriteCache[tileKey] = sprites;
            return true;
        }
    }
}
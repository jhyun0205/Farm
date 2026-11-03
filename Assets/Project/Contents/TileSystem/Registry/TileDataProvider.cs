using System.Collections.Generic;
using CoreEngine.GameData;
using CoreEngine.TileSystem.Contracts;
using Farm.GameData.Item;
using UnityEngine;

namespace Farm.GameData.Tile
{
    public sealed class TileDataProvider : ITileDataProvider
    {
        private const int SINGLE_SPRITE_COUNT = 1;
        private const int CARDINAL4_SPRITE_COUNT = 16;
        private const int MAX_BLOB8_SPRITE_COUNT = 47;

        private readonly TableHandler<TileMetaTable, TileMetaRecord> _tileMetaHandler = new();
        private readonly TableHandler<ItemRangeTable, ItemRangeRecord> _itemRangeHandler = new();
        private readonly TableHandler<TileRuleSpriteTable, TileRuleSpriteRecord> _spriteRuleHandler = new();

        private readonly Dictionary<ulong, Sprite[]> _resolvedSpriteCache;

        public TileDataProvider(int initialTileCapacity = 64)
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

        public bool TryGetItemRange(ulong itemKey, out ItemRangeRecord record)
        {
            if (_itemRangeHandler.TryGetReferencingRecords(
                    ItemRangeTable.REF_INDEX_ITEM_FK,
                    itemKey,
                    out IReadOnlyList<ItemRangeRecord> records) &&
                records.Count > 0)
            {
                record = records[0];
                return true;
            }

            record = null;
            return false;
        }

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
                    out IReadOnlyList<TileRuleSpriteRecord> records) ||
                records.Count == 0)
            {
                _resolvedSpriteCache[tileKey] = null;
                sprites = null;
                return false;
            }

            int arrayLength = GetRequiredSpriteArrayLength(tileKey);
            sprites = new Sprite[arrayLength];

            for (int i = 0; i < records.Count; i++)
            {
                TileRuleSpriteRecord rec = records[i];
                if (rec.MaskIndex < sprites.Length)
                {
                    sprites[rec.MaskIndex] = rec.TileSprite;
                }
            }

            _resolvedSpriteCache[tileKey] = sprites;
            return true;
        }

        private int GetRequiredSpriteArrayLength(ulong tileKey)
        {
            if (_tileMetaHandler.TryGetRecord(tileKey, out TileMetaRecord metaRecord))
            {
                return metaRecord.RuleType switch
                {
                    BitmaskRuleType.None => SINGLE_SPRITE_COUNT,
                    BitmaskRuleType.Cardinal4 => CARDINAL4_SPRITE_COUNT,
                    BitmaskRuleType.Blob8 => MAX_BLOB8_SPRITE_COUNT,
                    _ => MAX_BLOB8_SPRITE_COUNT
                };
            }

            return MAX_BLOB8_SPRITE_COUNT;
        }
    }
}
using System.Collections.Generic;
using CoreEngine.TileSystem.Contracts;
using UnityEngine;

namespace Farm.GameData
{
    public sealed class TileDataRegistry : ITileDataProvider
    {
        private const int MAX_BLOB8_SPRITE_COUNT = 47;

        private readonly TileMetaTable _tileMetaTable;
        private readonly ItemRangeTable _itemRangeTable;
        private readonly TileRuleSpriteTable _spriteRuleTable;

        private readonly Dictionary<ulong, Sprite[]> _resolvedSpriteCache;

        public TileDataRegistry(
            TileMetaTable tileMetaTable,
            ItemRangeTable itemRangeTable,
            TileRuleSpriteTable spriteRuleTable,
            int initialTileCapacity = 64)
        {
            _tileMetaTable = tileMetaTable;
            _itemRangeTable = itemRangeTable;
            _spriteRuleTable = spriteRuleTable;
            _resolvedSpriteCache = new Dictionary<ulong, Sprite[]>(initialTileCapacity);
        }

        public bool TryGetTileCreationInfo(ulong tileId, out TileCreationInfo info)
        {
            TileMetaRecord record = _tileMetaTable != null ? _tileMetaTable.GetTypedRecord(tileId) : null;
            if (record != null)
            {
                info = record.ToCreationInfo();
                return true;
            }

            info = default;
            return false;
        }

        public bool TryGetTileMeta(ulong tileId, out TileMetaRecord record)
        {
            record = _tileMetaTable != null ? _tileMetaTable.GetTypedRecord(tileId) : null;
            return record != null;
        }

        public bool TryGetItemRange(ulong itemId, out ItemRangeRecord record)
        {
            record = _itemRangeTable != null ? _itemRangeTable.GetTypedRecord(itemId) : null;
            return record != null;
        }

        public Sprite GetTileSprite(ulong tileId, byte maskIndex)
        {
            if (!TryGetOrBuildSpriteArray(tileId, out Sprite[] sprites) || sprites.Length == 0)
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

        private bool TryGetOrBuildSpriteArray(ulong tileId, out Sprite[] sprites)
        {
            if (_resolvedSpriteCache.TryGetValue(tileId, out sprites))
            {
                return sprites != null;
            }

            if (_spriteRuleTable == null)
            {
                sprites = null;
                return false;
            }

            List<TileRuleSpriteRecord> records = _spriteRuleTable.GetSpritesByTileId(tileId);
            if (records == null || records.Count == 0)
            {
                _resolvedSpriteCache[tileId] = null;
                sprites = null;
                return false;
            }

            sprites = new Sprite[MAX_BLOB8_SPRITE_COUNT];
            for (int i = 0; i < records.Count; i++)
            {
                TileRuleSpriteRecord rec = records[i];
                if (rec.MaskIndex < MAX_BLOB8_SPRITE_COUNT)
                {
                    sprites[rec.MaskIndex] = rec.SpriteAssetId.Get();
                }
            }

            _resolvedSpriteCache[tileId] = sprites;
            return true;
        }
    }
}
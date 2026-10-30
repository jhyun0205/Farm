using System;
using System.Collections.Generic;
using CoreEngine.GameData;
using UnityEngine;

namespace Farm.GameData
{
    [Serializable]
    public class TileRuleSpriteRecord : BaseRecord
    {
        [TableColumn, SerializeField] protected byte maskIndex;
        [TableColumn, SerializeField] protected AssetId<Sprite, TileSpriteAssetRegistry> spriteAssetId;
        [SerializeField] protected ForeignKey<TileMetaRecord, TileMetaTable> tileKey;

        public ForeignKey<TileMetaRecord, TileMetaTable> TileKey => tileKey;
        public byte MaskIndex => maskIndex;
        public AssetId<Sprite, TileSpriteAssetRegistry> SpriteAssetId => spriteAssetId;
    }

    public class TileRuleSpriteTable : BaseTable<TileRuleSpriteRecord>
    {
        public const uint REF_INDEX_TILE_KEY = 0;

        protected override List<KeyValuePair<uint, GetForeignKeyHash>> GetReverseIndexBuilder()
        {
            return new List<KeyValuePair<uint, GetForeignKeyHash>>
            {
                new KeyValuePair<uint, GetForeignKeyHash>(
                    REF_INDEX_TILE_KEY,
                    record => record.TileKey.TargetHashedPK)
            };
        }

        public List<TileRuleSpriteRecord> GetSpritesByTileId(ulong tileId) =>
            GetReferencingRecords(REF_INDEX_TILE_KEY, tileId);
    }
}
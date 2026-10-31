using System;
using System.Collections.Generic;
using CoreEngine.GameData;
using UnityEngine;

namespace Farm.GameData.Tile
{
    using TileSpriteId = AssetId<Sprite, TileSpriteAssetRegistry>;

    [Serializable]
    public class TileRuleSpriteRecord : BaseRecord
    {
        #region Schema

        [TableColumn, SerializeField] protected int tileId;
        [TableColumn, SerializeField] protected string tileName;
        [SerializeField] protected ForeignKey<TileMetaRecord, TileMetaTable> tileFK;
        [TableColumn, SerializeField] protected byte maskIndex;
        [TableColumn, SerializeField] protected TileSpriteId spriteAssetId;

        #endregion

        public int TileId => tileId;
        public string TileName => tileName;
        public ForeignKey<TileMetaRecord, TileMetaTable> TileFK => tileFK;
        public byte MaskIndex => maskIndex;
        public Sprite TileSprite => spriteAssetId.Get();
    }

    public class TileRuleSpriteTable : BaseTable<TileRuleSpriteRecord>
    {
        public const uint REF_INDEX_TILE_FK = 0;

        protected override List<KeyValuePair<uint, GetForeignKeyHash>> GetReverseIndexBuilder()
        {
            return new List<KeyValuePair<uint, GetForeignKeyHash>>
            {
                new KeyValuePair<uint, GetForeignKeyHash>(
                    REF_INDEX_TILE_FK,
                    record => record.TileFK.TargetHashedPK)
            };
        }
    }
}
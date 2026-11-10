using CoreEngine.GameData;
using UnityEngine.Tilemaps;
using UnityEngine;
using System;
using System.Collections.Generic;

namespace Farm.GameData.LevelDesign
{
    using TileBaseId = AssetId<TileBase, TileBaseRegistry>;
    using SpriteId = AssetId<Sprite, SpriteRegistry>;

    [Serializable]
    public class SeasonalTileRecord : BaseRecord
    {
        #region Schema
        [TableColumn, SerializeField] protected TileBaseId tileBaseId;
        [TableColumn, SerializeField] protected string tileName;
        [TableColumn, SerializeField] protected SpriteId springSpriteId;
        [TableColumn, SerializeField] protected SpriteId summerSpriteId;
        [TableColumn, SerializeField] protected SpriteId fallSpriteId;
        [TableColumn, SerializeField] protected SpriteId winterSpriteId;
        #endregion

        public TileBaseId TileBaseId => tileBaseId;
        public SpriteId SpringSpriteId => springSpriteId;
        public SpriteId SummerSpriteId => summerSpriteId;
        public SpriteId FallSpriteId => fallSpriteId;
        public SpriteId WinterSpriteId => winterSpriteId;
    }
    public class SeasonalTileTable : BaseTable<SeasonalTileRecord>
    {

    }
}


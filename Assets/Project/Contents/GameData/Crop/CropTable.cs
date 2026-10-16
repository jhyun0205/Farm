using CoreEngine.GameData;
using Farm.GameData.Item;
using System.Collections.Generic;
using UnityEngine;

namespace Farm.GameData.Crop
{
    public enum Season {Spring, Summer, Autumn, Winter }

    [System.Serializable]
    public class CropRecord : BaseRecord
    {
        #region Schema
        // 기본 정보 및 수확물 아이템 식별 정보
        [TableColumn, SerializeField] private int id;
        [TableColumn, SerializeField] private string name;

        // 재배 정보
        [TableColumn, SerializeField] private Season season; // 제철인 계절
        [TableColumn, SerializeField] private int daysToMaturity; // 수확까지 걸리는 날짜 (계산할때는 시간으로 환산)

        // 성장 리소스
        [TableColumn, SerializeField] private AssetId<Sprite, SpriteRegistry>[] growthSpriteIds;

        // 씨앗 아이템 참조
        [TableColumn, SerializeField] private int seedId;
        [TableColumn, SerializeField] private string seedName;

        /// <summary>
        /// <b>Coposite of</b><br/>
        /// <see cref="seedId"/>, <see cref="seedName"/>
        /// </summary>
        [SerializeField] private ForeignKey<ItemRecord, ItemTable> seedItemFK;

        /// <summary>
        /// <b>Coposite of</b><br/>
        /// <see cref="id"/>, <see cref="name"/>
        /// </summary>
        [SerializeField] private ForeignKey<ItemRecord, ItemTable> cropItemFK;

        #endregion


        public string CropName => name;
        public Season Season => season;
        public int DaysToMaturity => daysToMaturity;

        public ForeignKey<ItemRecord, ItemTable> SeedItemFK => seedItemFK;
        public ForeignKey<ItemRecord, ItemTable> CropItemFK => cropItemFK;

        public Sprite[] GetSprites()
        {
            int size = growthSpriteIds.Length;
            Sprite[] result = new Sprite[size];
            for(int i = 0; i<size; ++i)
            {
                result[i] = growthSpriteIds[i].Get();
            }
            return result;
        }
    }
    public class CropTable : BaseTable<CropRecord>
    {
        public const uint INDEX_SEED = 0;
        //public const uint INDEX_CROP = 1;
        protected override List<KeyValuePair<uint, GetForeignKeyHash>> GetReverseIndexBuilder()
        {
            return new()
            {
                new(INDEX_SEED, record => record.SeedItemFK.ID),
                //new(INDEX_CROP, record => record.CropItemFK.ID)
            };
        }
    }

}

using System;
using System.Collections.Generic;
using CoreEngine.GameData;
using UnityEngine;

namespace Farm.GameData.Item
{
    using ItemFK = ForeignKey<ItemRecord, ItemTable>;

    [Serializable]
    public class ItemRangeRecord : BaseRecord
    {
        #region Schema

        [TableColumn, SerializeField] protected int itemId;
        [TableColumn, SerializeField] protected string itemName;
        [TableColumn, SerializeField] protected int interactionRange;
        [TableColumn, SerializeField] protected int targetCategoryMask;
        [SerializeField] protected ItemFK itemFK;

        #endregion

        public int ItemId => itemId;
        public string ItemName => itemName;
        public int InteractionRange => interactionRange;
        public int TargetCategoryMask => targetCategoryMask;
        public ItemFK ItemFK => itemFK;
        public ItemRecord Item => itemFK.Get();
    }

    public class ItemRangeTable : BaseTable<ItemRangeRecord>
    {
        public const uint REF_INDEX_ITEM_FK = 0;

        protected override List<KeyValuePair<uint, GetForeignKeyHash>> GetReverseIndexBuilder()
        {
            return new List<KeyValuePair<uint, GetForeignKeyHash>>
            {
                new KeyValuePair<uint, GetForeignKeyHash>(REF_INDEX_ITEM_FK, record => record.ItemFK.TargetHashedPK)
            };
        }
    }
}
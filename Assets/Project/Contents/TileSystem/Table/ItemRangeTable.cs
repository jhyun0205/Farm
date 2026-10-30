using System;
using CoreEngine.GameData;
using Farm.GameData.Item;
using UnityEngine;

namespace Farm.GameData
{
    [Serializable]
    public class ItemRangeRecord : BaseRecord
    {
        #region Schema

        [TableColumn, SerializeField] protected int itemId;
        [TableColumn, SerializeField] protected string itemName;
        [SerializeField] protected ForeignKey<ItemRecord, ItemTable> itemFK;
        [TableColumn, SerializeField] protected int interactionRange;
        [TableColumn, SerializeField] protected int targetCategoryMask;

        #endregion

        public int ItemId => itemId;
        public string ItemName => itemName;
        public ItemRecord Item => itemFK.Get();
        public int InteractionRange => interactionRange;
        public int TargetCategoryMask => targetCategoryMask;
    }

    public class ItemRangeTable : BaseTable<ItemRangeRecord>
    {
    }
}
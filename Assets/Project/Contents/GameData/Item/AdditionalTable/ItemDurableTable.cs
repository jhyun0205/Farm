using CoreEngine.GameData;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;


namespace Farm.GameData.Item
{
    using ItemFK = ForeignKey<ItemRecord, ItemTable>;

    [System.Serializable]
    public class ItemDurableRecord : BaseRecord
    {
        #region Schema

        [TableColumn, SerializeField] private int itemId;
        [TableColumn, SerializeField] private string itemName;
        [TableColumn, SerializeField] private int maxDurability;
        [SerializeField] private ItemFK itemFK;

        #endregion

        public int MaxDurability => maxDurability;

        public ItemFK ItemFK => itemFK;
    }

    public class ItemDurableTable : BaseTable<ItemDurableRecord>
    {
        public const uint INDEX_DURABLE = 0;
        protected override List<KeyValuePair<uint, GetForeignKeyHash>> GetReverseIndexBuilder()
        {
            return new()
            {
                new(INDEX_DURABLE, record => record.ItemFK.TargetHashedPK),
            };
        }
    }

}

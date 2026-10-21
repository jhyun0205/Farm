using CoreEngine.GameData;
using System.Collections.Generic;
using UnityEngine;

namespace Farm.GameData.Item
{
    [System.Serializable]
    public class ItemIndexedEffectRecord : ItemEffectRecord
    {
        [TableColumn, SerializeField] private int index;
        public int Index => index;
    }

    public class ItemEffectOnAnimTable : BaseTable<ItemIndexedEffectRecord>
    {
        public const uint INDEX_EFFECT = 0;

        protected override List<KeyValuePair<uint, GetForeignKeyHash>> GetReverseIndexBuilder()
        {
            return new()
            {
                new(INDEX_EFFECT, record => record.ItemFK.TargetHashedPK),
            };
        }
    }
}

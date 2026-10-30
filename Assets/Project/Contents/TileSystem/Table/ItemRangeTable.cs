using System;
using CoreEngine.GameData;
using UnityEngine;

namespace Farm.GameData
{
    [Serializable]
    public class ItemRangeRecord : BaseRecord
    {
        [TableColumn, SerializeField] protected int interactionRange;
        [TableColumn, SerializeField] protected int targetCategoryMask;

        public int InteractionRange => interactionRange;
        public int TargetCategoryMask => targetCategoryMask;
    }

    public class ItemRangeTable : BaseTable<ItemRangeRecord>
    {
        public ItemRangeRecord GetTypedRecord(ulong id) => GetRecord(id) as ItemRangeRecord;
    }
}
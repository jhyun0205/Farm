using CoreEngine.GameData;
using UnityEngine;

namespace Farm.GameData.Item
{
    using ItemConditionAssetID = AssetId<ItemCondition, ItemConditionRegistry>;

    public class ItemConditionRecord : BaseRecord
    {
        [TableColumn, SerializeField] private int id;
        [TableColumn, SerializeField] private string name;

        [TableColumn, SerializeField] private ItemConditionAssetID ConditionID;

        public ItemCondition GetCondition() => ConditionID.Get();

    }
    public class ItemConditionTable : BaseTable<ItemConditionRecord>
    {

    }
}


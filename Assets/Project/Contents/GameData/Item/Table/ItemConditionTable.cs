using CoreEngine.GameData;
using UnityEngine;

namespace Farm.GameData.Item
{
    public class ItemConditionRecord : BaseRecord
    {
        [TableColumn, SerializeField] private int id;
        [TableColumn, SerializeField] private string name;

        [TableColumn, SerializeField] private AssetId<Condition, ConditionRegistry> ConditionID;

        public Condition GetCondition() => ConditionID.Get();

        protected override string GetPrimaryKey()
        {
            throw new System.NotImplementedException();
        }
    }
    public class ItemConditionTable : BaseTable<ItemConditionRecord>
    {

    }
}


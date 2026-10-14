using CoreEngine.GameData;
using System.Runtime.CompilerServices;
using UnityEngine;


namespace Farm.GameData.Item
{
    public class ItemDurableRecord : BaseRecord
    {
        [TableColumn, SerializeField] private int id;
        [TableColumn, SerializeField] private string name;
        [TableColumn, SerializeField] private int maxDurability;

        public int MaxDurability => maxDurability;

        protected override string GetPrimaryKey()
        {
            throw new System.NotImplementedException();
        }
    }

    public class ItemDurableTable : BaseTable<ItemDurableRecord>
    {

    }

}

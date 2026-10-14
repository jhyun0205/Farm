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

    }

    public class ItemDurableTable : BaseTable<ItemDurableRecord>
    {

    }

}

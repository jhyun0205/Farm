using CoreEngine;
using CoreEngine.GameData;
using CoreEngine.Hub;
using UnityEngine;



namespace Farm.GameData.Item
{
    public enum ItemType
    {
        Material,    // 재료
        Consumable, // 소모품
        Equipment,  // 장비
    }

    [System.Serializable]
    public class ItemRecord : BaseRecord
    {
        #region Schema

        [TableColumn, SerializeField] private int id;
        [TableColumn, SerializeField] private string name;
        [TableColumn, SerializeField] private ItemType itemType;
        [TableColumn, SerializeField] private string description;
        [TableColumn, SerializeField] private int maxStack; // -1이면 무한대
        [TableColumn, SerializeField] private int sellPrice; // -1이면 판매불가

        #endregion


        public int Id => id;
        public string Name => name;
        public string Description => description;
        public int MaxStack => maxStack;
        public int SellPrice => sellPrice;
        public ItemType ItemType => itemType;

    }

    public class ItemTable : BaseTable<ItemRecord>
    {

    }
}
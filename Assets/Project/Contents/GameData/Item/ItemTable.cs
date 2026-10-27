using CoreEngine;
using CoreEngine.GameData;
using CoreEngine.Hub;
using UnityEngine;



namespace Farm.GameData.Item
{
    using ItemSpriteId = AssetId<Sprite, TempSpriteRegistry>;

    public enum ItemType
    {
        Equipment = 0,  // 장비
        Consumable = 1, // 소모품
        Material = 2,    // 재료
    }

    [System.Serializable]
    public class ItemRecord : BaseRecord
    {
        #region Schema

        [TableColumn, SerializeField] private int id;
        [TableColumn, SerializeField] private string name;
        [TableColumn, SerializeField] private ItemType itemType;
        [TableColumn, SerializeField] private ItemSpriteId itemSpriteId;
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
        public Sprite ItemSprite => itemSpriteId.Get();

    }

    public class ItemTable : BaseTable<ItemRecord>
    {

    }
}
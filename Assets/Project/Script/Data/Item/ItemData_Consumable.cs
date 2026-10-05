
namespace Farm.GameData.Item
{
    //public enum ConsumableType
    //{
    //    Seed,
    //}

    [System.Serializable]
    public class ItemData_Consumable : ItemData
    {
        public ConsumableType consumableType;
        public float effectValue;
    }
}

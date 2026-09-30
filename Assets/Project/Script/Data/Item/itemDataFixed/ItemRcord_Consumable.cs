
using CoreEngine.GameData;
using UnityEngine;

namespace Farm.GameData.Item
{
    public enum ConsumableType
    {
        Seed,
    }


    public class ItemRcord_Consumable : ItemRcord
    {
        [TableColumn, SerializeField] private ConsumableType consumableType;
        [TableColumn, SerializeField] private float effectValue;

        public ConsumableType ConsumableType => consumableType;
        public float EffectValue => effectValue;
    }
}

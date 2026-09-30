using CoreEngine.GameData;
using UnityEngine;

namespace Farm.GameData.Item
{
    public class ItemRcord_Equipment : ItemRcord
    {
        [TableColumn, SerializeField] private int maxDurability;
        public int MaxDurability => maxDurability;

    }
}

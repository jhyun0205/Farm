using CoreEngine.GameData;
using UnityEngine;

namespace Farm.GameData.Item
{
    [System.Serializable]
    public class RecordItemEquipment : RecordBaseItemUsable
    {
        [TableColumn, SerializeField] private int maxDurability;
        public int MaxDurability => maxDurability;

    }
}

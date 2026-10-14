using CoreEngine.GameData;
using Farm.Character;
using UnityEngine;

namespace Farm.GameData.Item
{
    public class ItemEffectRecord : BaseRecord
    {
        [TableColumn, SerializeField] private int id;
        [TableColumn, SerializeField] private string name;

        [TableColumn, SerializeField] private AssetId<ItemEffect, ItemEffectRegistry> itemEffectID;
        [TableColumn, SerializeField] private int[] effectValues;

        private ForeignKey<ItemRecord, ItemTable> itemFK;

        public void ApplyEffect(BaseCharacter character)
        {
            itemEffectID.Get().Apply(character, itemFK.Get(), effectValues);
        }

    }
    public class ItemEffectTable : BaseTable<ItemEffectRecord>
    {

    }
}


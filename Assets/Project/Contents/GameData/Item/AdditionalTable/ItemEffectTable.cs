using CoreEngine.GameData;
using CoreEngine.Helpers;
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
            if(itemFK.Get() is ItemRecord item)
            {
                itemEffectID.Get().Apply(character, item, effectValues);
            }
            else
            {
                LogHelper.LogWarning($"{nameof(ApplyEffect)}: {nameof(itemFK)}.{nameof(itemFK.Get)} 실패");
            }
        }

    }
    public class ItemEffectTable : BaseTable<ItemEffectRecord>
    {

    }
}


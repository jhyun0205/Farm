using CoreEngine.GameData;
using CoreEngine.Helpers;
using Farm.Character;
using UnityEngine;

namespace Farm.GameData.Item
{
    [System.Serializable]
    public class ItemEffectRecord : BaseRecord
    {
        [TableColumn, SerializeField] private int itemId;
        [TableColumn, SerializeField] private string itemName;

        [TableColumn, SerializeField] private AssetId<ItemEffect, ItemEffectRegistry> itemEffectId;
        [TableColumn, SerializeField] private int[] effectValues;
        [SerializeField] private ForeignKey<ItemRecord, ItemTable> itemFK;


        public void ApplyEffect(BaseCharacter character)
        {
            if(itemFK.Get() is ItemRecord item)
            {
                itemEffectId.Get().Apply(character, item, effectValues);
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


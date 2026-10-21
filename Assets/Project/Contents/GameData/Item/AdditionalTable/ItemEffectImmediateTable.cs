using CoreEngine.GameData;
using CoreEngine.Helpers;
using Farm.Character;
using System.Collections.Generic;
using UnityEngine;

namespace Farm.GameData.Item
{
    using ItemEffectId = AssetId<ItemEffect, ItemEffectRegistry>;
    using ItemFK = ForeignKey<ItemRecord, ItemTable>;
    
    [System.Serializable]
    public class ItemEffectRecord : BaseRecord
    {
        #region Schema

        [TableColumn, SerializeField] private int itemId;
        [TableColumn, SerializeField] private string itemName;
        [TableColumn, SerializeField] private ItemEffectId itemEffectId;
        [TableColumn, SerializeField] private int[] effectValues;

        [SerializeField] private ItemFK itemFK;

        #endregion

        public ItemFK ItemFK => itemFK;

        public void ApplyEffect(BaseCharacter character)
        {
            if (itemFK.Get() is ItemRecord item)
            {
                itemEffectId.Get().Apply(character, item, effectValues);
            }
            else
            {
                LogHelper.LogWarning($"{nameof(ApplyEffect)}: {nameof(itemFK)}.{nameof(itemFK.Get)} 실패");
            }
        }
    }

    public class ItemEffectImmediateTable : BaseTable<ItemEffectRecord>
    {
        public const uint INDEX_EFFECT = 0;

        protected override List<KeyValuePair<uint, GetForeignKeyHash>> GetReverseIndexBuilder()
        {
            return new()
            {
                new(INDEX_EFFECT, record => record.ItemFK.TargetHashedPK),
            };
        }
    }
}
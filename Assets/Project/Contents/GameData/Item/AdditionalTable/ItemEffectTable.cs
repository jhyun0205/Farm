using CoreEngine.GameData;
using CoreEngine.Helpers;
using Farm.Character;
using System.Collections.Generic;
using UnityEngine;

namespace Farm.GameData.Item
{
    public enum EffectTriggerType
    {
        OnUse,         // 클릭 시 즉시 발동
        OnAnimEvent    // 애니메이션 이벤트 타이밍에 발동
    }

    [System.Serializable]
    public class ItemEffectRecord : BaseRecord
    {
        [TableColumn, SerializeField] private int itemId;
        [TableColumn, SerializeField] private string itemName;

        // [추가됨] 이펙트 발동 시점 구분
        [TableColumn, SerializeField] private EffectTriggerType triggerType;

        [TableColumn, SerializeField] private AssetId<ItemEffect, ItemEffectRegistry> itemEffectId;
        [TableColumn, SerializeField] private int[] effectValues;
        [SerializeField] private ForeignKey<ItemRecord, ItemTable> itemFK;

        public EffectTriggerType TriggerType => triggerType;
        public ForeignKey<ItemRecord, ItemTable> ItemFK => itemFK;

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

    public class ItemEffectTable : BaseTable<ItemEffectRecord>
    {
        // [추가됨] 아이템 ID 기반으로 이펙트를 빠르게 찾기 위한 역조회 인덱스
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
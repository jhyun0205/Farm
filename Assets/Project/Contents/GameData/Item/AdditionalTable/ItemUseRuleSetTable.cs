using CoreEngine.GameData;
using Farm.Character;
using UnityEngine;

namespace Farm.GameData.Item
{
    using static UnityEngine.UIElements.UxmlAttributeDescription;
    using ItemUseRuleAssetID = AssetId<ItemUseRule, ItemUseRuleRegistry>;

    public class ItemUseRuleSetRecord : BaseRecord
    {
        [TableColumn, SerializeField] private int itemId;
        [TableColumn, SerializeField] private string itemName;

        [TableColumn, SerializeField] private ItemUseRuleAssetID[] requirements;
        [TableColumn, SerializeField] private ItemUseRuleAssetID[] restrictions;

        private ForeignKey<ItemRecord, ItemTable> itemFK;

        public bool CanUse(BaseCharacter itemUser)
        {
            ItemRecord item = itemFK.Get();

            // 사용 가능여부 확인
            if (requirements is not null)
            {
                for (int i = 0; i < requirements.Length; ++i)
                {
                    if (!IsSatisfied(itemUser, item, requirements[i])) return false;
                }
            }

            // 사용 불가능한지 확인
            if (restrictions is not null)
            {
                for (int i = 0; i < restrictions.Length; ++i)
                {
                    if (IsSatisfied(itemUser, item, restrictions[i])) return false;
                }
            }

            return true;
        }

        private bool IsSatisfied(BaseCharacter itemOwner, ItemRecord item, ItemUseRuleAssetID ruleAssetId)
        {
            if(ruleAssetId.Get() is ItemUseRule rule)
            {
                return rule.IsSatisfied(itemOwner, item);
            }
            return false;
        }

    }
    public class ItemUseRuleSetTable : BaseTable<ItemUseRuleSetRecord>
    {

    }
}


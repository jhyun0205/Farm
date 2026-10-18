using System;
using CoreEngine.GameData;
using CoreEngine.Helpers;
using Farm.Character;

namespace Farm.GameData.Item
{
    [Serializable]
    public class ItemDataToSave : IDataToSave
    {
        public int amount;      // 아이템 개수
        public int durability;  // 아이템 내구도 (장비 외에는 -1)
        public int slotIndex;   // 퀵슬롯 위치 (0 ~ 8)
    }

    public class ItemInstance : BaseDataInstance<ItemRecord, ItemDataToSave>
    {
        // 관계형 테이블 조회용 정적 핸들러 (런타임 캐시 공유)
        private static readonly TableHandler<ItemDurableTable, ItemDurableRecord> _durableTable = new();
        private static readonly TableHandler<ItemUseRuleSetTable, ItemUseRuleSetRecord> _ruleTable = new();

        public int MaxStack => Record?.MaxStack ?? 0;
        public int MaxDurability { get; private set; }
        public bool IsEquipment => Record != null && Record.ItemType == ItemType.Equipment;

        // 단일 슬롯 UI(ItemViewer)가 구독할 상태 변경 이벤트
        public event Action Event_OnRuntimeDataChanged;

        // 신규 획득 시 생성자
        public ItemInstance(ItemRecord record, int slotIndex) : base(record)
        {
            InitializeRelationalData();
            RuntimeData.slotIndex = slotIndex;
            RuntimeData.amount = 0;
            RuntimeData.durability = IsEquipment ? MaxDurability : -1;
        }

        // 세이브 데이터 로드 시 생성자
        public ItemInstance(ItemRecord record, ItemDataToSave runtimeData) : base(record, runtimeData)
        {
            InitializeRelationalData();
        }

        private void InitializeRelationalData()
        {
            MaxDurability = 0;
            if (IsEquipment && _durableTable.TryGetReferencingRecords(ItemDurableTable.INDEX_DURABLE, Record, out var durables))
            {
                MaxDurability = durables[0].MaxDurability;
            }
        }

        public bool CanAddMore()
        {
            if (MaxStack == -1) return true; // -1은 무한 스택
            return RuntimeData.amount < MaxStack;
        }

        public virtual bool TryUse(BaseCharacter user, int count)
        {
            if (Record == null) return false;

            // 내구도 조건 먼저 확인 (비용이 싼 연산 우선)
            if (IsEquipment)
            {
                if (RuntimeData.durability < count) return false;
            }
            else
            {
                if (RuntimeData.amount < count) return false;
            }

            // 2. ItemUseRuleSetTable에 등록된 조건/제약 확인
            if (_ruleTable.TryGetReferencingRecords(ItemUseRuleSetTable.INDEX_RULE, Record, out var ruleSets))
            {
                for (int i = 0; i < ruleSets.Count; i++)
                {
                    if (!ruleSets[i].CanUse(user)) return false;
                }
            }

            return true;
        }

        public virtual void Push(int pushAmount, out int remain)
        {
            if (pushAmount < 1)
            {
                LogHelper.LogError("pushAmount 개수가 음수이거나 0임");
                remain = 0;
                return;
            }

            AddAndGetExcess(ref RuntimeData.amount, pushAmount, MaxStack, out remain);

            if (IsEquipment)
            {
                RuntimeData.durability = MaxDurability;
            }

            Event_OnRuntimeDataChanged?.Invoke();
        }

        public virtual void Pop(int popAmount, out int remain)
        {
            if (popAmount < 1)
            {
                LogHelper.LogError("popAmount 개수가 음수이거나 0임");
                remain = 0;
                return;
            }

            RuntimeData.amount -= popAmount;
            remain = 0;

            if (RuntimeData.amount < 0)
            {
                remain = -RuntimeData.amount;
                RuntimeData.amount = 0;
            }

            Event_OnRuntimeDataChanged?.Invoke();
        }

        public void ReduceDurability(int count = 1)
        {
            if (!IsEquipment)
            {
                LogHelper.LogError("장비가 아닌 객체의 내구도 변화 시도!");
                return;
            }

            RuntimeData.durability = Math.Max(0, RuntimeData.durability - count);
            Event_OnRuntimeDataChanged?.Invoke();
        }

        public void RefillDurability()
        {
            if (!IsEquipment)
            {
                LogHelper.LogError("장비가 아닌 객체의 내구도 변화 시도!");
                return;
            }

            RuntimeData.durability = MaxDurability;
            Event_OnRuntimeDataChanged?.Invoke();
        }

        public void RefillDurability(int fillAmount, out int remain)
        {
            remain = 0;
            if (!IsEquipment)
            {
                LogHelper.LogError("장비가 아닌 객체의 내구도 변화 시도!");
                return;
            }
            if (fillAmount < 1)
            {
                LogHelper.LogError("fillAmount 개수가 음수이거나 0임");
                return;
            }

            AddAndGetExcess(ref RuntimeData.durability, fillAmount, MaxDurability, out remain);
            Event_OnRuntimeDataChanged?.Invoke();
        }

        private void AddAndGetExcess(ref int cur, int plus, int max, out int remain)
        {
            cur += plus;
            remain = 0;

            // max가 -1(무한대)이 아닐 때만 상한선 체크
            if (max >= 0 && cur > max)
            {
                remain = cur - max;
                cur = max;
            }
        }

        public bool IsEmpty() => RuntimeData.amount <= 0;
    }
}
using CoreEngine.EventBus;
using CoreEngine.GameData;
using CoreEngine.Helpers;
using Farm.Character;

namespace Farm.GameData.Item
{
    

    /// <summary>
    /// 저장될 필드 묶음
    /// </summary>
    public class ItemDataToSave : IDataToSave
    {
        public int amount;      // 아이템 개수
        public int durability; // 아이템 내구도(장비에 사용 -> 사용 횟수 제한)
        public int slotIndex;    // 아이템이 들어있는 슬롯 인덱스
    }

    /// <summary>
    /// 직접 조작될 객체
    /// </summary>
    public class ItemInstance: BaseDataInstance<RecordBaseItem, ItemDataToSave>
    {
        public ItemInstance(RecordBaseItem record) : base(record)
        {
        }
        public ItemInstance(RecordBaseItem record, ItemDataToSave runtimeData) : base(record, runtimeData)
        {
        }

        // 슬롯이 비어있으면 0 반환
        public int maxStack => Record?.MaxStack ?? 0;

        // 장비가 아니거나 슬롯이 비어있으면 0 반환
        public int maxDurability => (Record as RecordItemEquipment)?.MaxDurability ?? 0;

        public virtual bool TryUse(BaseCharacter user, int count)
        {
            // 아이템 자체적으로 사용 가능한지 확인
            //bool able = connectData.TryUse(user);
            bool able = true;//Record.TryUse(user);
            if (!able) return false;

            // 개수나 내구도 같은 간단한 것들 확인
            //if (connectData is ItemData_Equipment)
            if (Record is RecordItemEquipment)
            {
                // 장비라면 count만큼 내구도 감소
                //if (durability < count) return false;
                if (DataToSave.durability < count) return false;
            }
            else
            {
                // 그 외 아이템이 사용된 경우
                //if (amount < count) return false;
                if (DataToSave.amount < count) return false;
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
            //AddAndGetExcess(ref amount, pushAmount, maxStack, out remain);
            AddAndGetExcess(ref DataToSave.amount, pushAmount, maxStack, out remain);

            //if (connectData is ItemData_Equipment)
            if (Record is RecordItemEquipment)
            {
                //durability = maxDurability;
                DataToSave.durability = maxDurability;
            }

            // ui에 변화 알림
            //Event_OnAmountChanged?.Invoke();
            //EventBus<ItemAmountChangeEvent>.Publish(new ItemAmountChangeEvent());
        }



        public virtual void Pop(int popAmount, out int remain)
        {
            if (popAmount < 1)
            {
                LogHelper.LogError("popAmount 개수가 음수이거나 0임");
                remain = 0;
                return;
            }

            //amount -= popAmount;
            DataToSave.amount -= popAmount;
            remain = 0;

            // 아이템 추가 후 최대치보다 많이 갖게 되면 나머지는 뱉어냄
            //if (amount < 0)
            if (DataToSave.amount < 0)
            {
                //remain = amount * -1;
                //amount = 0;
                remain = DataToSave.amount * -1;
                DataToSave.amount = 0;
            }
            //if (amount == 0)
            //if (DataToSave.amount == 0)
            //{
            //    Clear();
            //}

            // ui에 변화 알림
            //Event_OnAmountChanged?.Invoke();
            //EventBus<ItemAmountChangeEvent>.Publish(new ItemAmountChangeEvent());

        }

        public void ReduceDurability(int count = 1)
        {
            //if(currentObject is not Item_Equipment)
            //{
            //    Debug.LogError("장비가 아닌 객체의 내구도 변화 시도!");
            //    return false;
            //}
            //if (durability - count < 0) return false;

            DataToSave.durability -= count;

            //Event_OnDurabilityChanged?.Invoke();
            //EventBus<ItemDurabilityChangeEvent>.Publish(new ItemDurabilityChangeEvent());
            //return true;
        }

        public void RefillDurability()
        {
            //if (connectData is not ItemData_Equipment)
            //{
            //    Debug.LogError("장비가 아닌 객체의 내구도 변화 시도!");
            //    return;
            //}
            //durability = maxDurability;
            //Event_OnDurabilityChanged?.Invoke();

            if (Record is not RecordItemEquipment)
            {
                LogHelper.LogError("장비가 아닌 객체의 내구도 변화 시도!");
                return;
            }
            DataToSave.durability = maxDurability;
            //EventBus<ItemDurabilityChangeEvent>.Publish(new ItemDurabilityChangeEvent());
        }

        public void RefillDurability(int fillAmount, out int remain)
        {
            remain = 0;
            //if (connectData is not ItemData_Equipment)
            if (Record is not RecordItemEquipment)
            {
                LogHelper.LogError("장비가 아닌 객체의 내구도 변화 시도!");
                return;
            }
            if (fillAmount < 1)
            {
                LogHelper.LogError("fillAmount 개수가 음수이거나 0임");
                return;
            }
            //AddAndGetExcess(ref durability, fillAmount, maxDurability, out remain);
            AddAndGetExcess(ref DataToSave.durability, fillAmount, maxDurability, out remain);

            // ui에 변화 알림
            //Event_OnDurabilityChanged?.Invoke();
            ///*EventBus<ItemDurabilityChangeEvent>.Publish(new ItemDurabilityChangeEvent());*/
        }

        private void AddAndGetExcess(ref int cur, int plus, int max, out int remain)
        {
            cur += plus;
            remain = 0;

            // 최대치보다 많이 갖게 되면 나머지는 뱉어냄
            if (cur > max)
            {
                remain = cur - max;
                cur = max;
            }
        }

        // 슬롯이 비어있는지 확인하는 헬퍼 함수
        //public bool IsEmpty() => connectData == null || amount <= 0;
        public bool IsEmpty() => DataToSave.amount <= 0;

        //public void Clear()
        //{
        //    connectData = null;

        //    //amount = 0;
        //    DataToSave.amount = 0;

        //    //Event_OnClear?.Invoke();
        //    EventBus<ItemClearEvent>.Publish(new ItemClearEvent());
        //}

        //public override ItemInstance Set(ItemInstance newObject)
        //{
        //    Clear();
        //    base.Set(newObject); // 부모 클래스의 기본 Set 기능(currentObject 할당) 실행

        //    if (newObject == null)
        //    {
        //        return null;
        //    }

        //    // 아이템 종류에 따라 초기값 셋팅
        //    //if (newObject is ItemData_Equipment eq)
        //    if (newObject.Record is RecordItemEquipment eq)
        //    {
        //        //durability = eq.maxDurability; // 획득 시 내구도 꽉 채우기
        //        newObject.DataToSave.durability = eq.MaxDurability; // 획득 시 내구도 꽉 채우기
        //    }
        //    else
        //    {
        //        // 소모품/재료는 내구도 0
        //        // amount는 Push나 Pop에서 알아서 관리할 테니 놔둠
        //        //durability = 0;
        //        newObject.DataToSave.durability = -1;
        //    }

        //    return connectData;
        //}
    }

    
}


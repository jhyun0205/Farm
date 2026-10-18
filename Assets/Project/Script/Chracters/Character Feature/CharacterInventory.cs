using CoreEngine.EventBus;
using CoreEngine.Extensions;
using CoreEngine.GameData;
using CoreEngine.Helpers;
using CoreEngine.Interface;
using Farm.GameData;
using Farm.GameData.Item;
using Farm.UI.Item;
using System;
using System.Collections.Generic;

namespace Farm.Character
{
    [Serializable]
    public class CharacterInventory : BaseDataHandler<ItemTable, ItemRecord, ItemInstance, ItemDataToSave>
    {
        public const int QUICK_SLOT_SIZE = 9;

        protected int curItemIndex;
        protected ItemInstance[] items = new ItemInstance[QUICK_SLOT_SIZE];
        public IReadOnlyList<ItemInstance> Items => items;

        private readonly InterfaceReceiver<IQuickSlotUpdate> _quickSlotUpdateReceiver = new();
        private CharacterActionController actionController;

        protected override void OnInitialized()
        {
            base.OnInitialized();
            Array.Clear(items, 0, items.Length);
            curItemIndex = 0;
            Host.TryGetFeature(out actionController);
        }

        public override void OnSpawn()
        {
            base.OnSpawn();
            _quickSlotUpdateReceiver.Bind();

#if UNITY_EDITOR
            // 테스트 획득 예시 (자동 생성된 ulong 키 사용)
            // AcquireItem(ItemRecordKeys._0_괭이, 1);
            // AcquireItem(ItemRecordKeys._100_볍씨, 50);
#endif
        }

        public override void OnDespawn()
        {
            _quickSlotUpdateReceiver.Unbind();
            base.OnDespawn();
        }

        public override void LoadData()
        {
            Array.Clear(items, 0, items.Length);

            var loadedList = repository.Load(tableHandler, (record, saveData) => new ItemInstance(record, saveData));
            foreach (var instance in loadedList)
            {
                int slotIdx = instance.RuntimeData.slotIndex;
                if (0 <= slotIdx && slotIdx < items.Length && items[slotIdx] == null)
                {
                    items[slotIdx] = instance;
                }
                else
                {
                    LogHelper.LogWarning($"유효하지 않거나 중복된 슬롯 인덱스({slotIdx}) 로드됨.");
                }
            }

            if (_quickSlotUpdateReceiver.TryGet(out var quickSlotUpdate))
            {
                for (int i = 0; i < items.Length; i++)
                {
                    quickSlotUpdate.OnItemSlotChanged(i);
                }
            }
        }

        public ItemInstance GetItem(int index)
        {
            return items.TryGet(index, out var slot) ? slot : null;
        }

        public void ScrollSlot(float scrollDelta)
        {
            if (scrollDelta == 0) return;

            if (scrollDelta > 0) curItemIndex--;
            else curItemIndex++;

            if (curItemIndex < 0) curItemIndex = items.Length - 1;
            else if (curItemIndex >= items.Length) curItemIndex = 0;

            NotifySelectedSlotChanged();
        }

        public void SetSelectedSlot(int index)
        {
            if (0 <= index && index < items.Length)
            {
                curItemIndex = index;
                NotifySelectedSlotChanged();
            }
            else
            {
                LogHelper.LogWarning($"Selected slot out of range: {index}");
            }
        }

        private void NotifySelectedSlotChanged()
        {
            if (_quickSlotUpdateReceiver.TryGet(out var quickSlotUpdate))
            {
                quickSlotUpdate.OnSelectedSlotChanged(curItemIndex);
            }
            actionController?.EquipItem(curItemIndex);
            EventBus<SelectedSlotChangedEvent>.Publish(new SelectedSlotChangedEvent { SlotIndex = curItemIndex });
        }

        /// <summary>
        /// 필드에서 아이템 획득 시 호출 (ItemRecordKeys의 ulong 해시 전달)
        /// </summary>
        public void AcquireItem(ulong itemKey, int count = 1)
        {
            if (count <= 0) return;
            if (!tableHandler.TryGetRecord(itemKey, out ItemRecord record)) return;

            int remainCount = count;
            _quickSlotUpdateReceiver.TryGet(out var quickSlotUpdate);

            // 1단계: 이미 같은 아이템이 들어있고 여유 공간이 있는 슬롯들에 먼저 채움
            for (int i = 0; i < items.Length && remainCount > 0; i++)
            {
                ItemInstance slot = items[i];
                if (slot != null && slot.Record.HashedPK == itemKey && slot.CanAddMore())
                {
                    slot.Push(remainCount, out remainCount);
                    // 이미 존재하는 인스턴스의 수량만 변했으므로 ItemInstance 내부 이벤트로도 갱신되지만 명시적 호출 유지 가능
                }
            }

            // 2단계: 남은 수량이 있다면 빈 슬롯(null)들을 순회하며 새 인스턴스 생성 (다중 슬롯 오버플로우 대응)
            for (int i = 0; i < items.Length && remainCount > 0; i++)
            {
                if (items[i] == null)
                {
                    ItemInstance newInstance = new ItemInstance(record, i);
                    newInstance.Push(remainCount, out remainCount);

                    items[i] = newInstance;
                    repository.Add(newInstance);

                    // null이었던 슬롯에 새 객체가 할당되었으므로 UI에 재연결 요청
                    quickSlotUpdate?.OnItemSlotChanged(i);
                }
            }

            if (remainCount > 0)
            {
                LogHelper.LogWarning($"인벤토리(퀵슬롯)가 꽉 차서 {record.Name} {remainCount}개를 획득하지 못했습니다.");
                // TODO: 남은 수량(remainCount)만큼 필드에 드랍 아이템 생성
            }
        }

        public void UseItem(int count = 1)
        {
            if (curItemIndex < 0 || curItemIndex >= items.Length) return;

            ItemInstance item = items[curItemIndex];
            if (item == null || item.IsEmpty())
            {
                LogHelper.Log("빈 슬롯입니다.");
                return;
            }

            if (item.TryUse(Host as BaseCharacter, count))
            {
                if (item.Record.ItemType == ItemType.Equipment)
                {
                    item.ReduceDurability(count);
                }
                else if (item.Record.ItemType == ItemType.Consumable)
                {
                    item.Pop(count, out _);

                    // 소모품을 모두 사용했다면 저장소에서 제거하고 슬롯을 null로 비움
                    if (item.IsEmpty())
                    {
                        repository.Remove(item);
                        items[curItemIndex] = null;

                        if (_quickSlotUpdateReceiver.TryGet(out var quickSlotUpdate))
                        {
                            quickSlotUpdate.OnItemSlotChanged(curItemIndex);
                        }
                    }
                }

                // TODO: ItemEffectTable에 연결된 즉시 발동 효과(ApplyEffect)가 있다면 여기서 실행 가능
                actionController?.OnItemUseInput();
            }
            else
            {
                LogHelper.LogWarning("사용 실패시 효과음 출력 바람");
            }
        }

        public struct SelectedSlotChangedEvent : IEvent
        {
            public int SlotIndex;
        }
    }
}
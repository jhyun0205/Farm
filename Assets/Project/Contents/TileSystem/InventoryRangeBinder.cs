using System;
using CoreEngine.Actor;
using CoreEngine.EventBus;
using Farm.Character;
using Farm.GameData.Item;
using Farm.GameData.Tile;
using UnityEngine;

namespace Farm.Systems.TileContent
{
    /// <summary>
    /// CharacterInventory의 슬롯 변경 이벤트를 구독하여 
    /// 빈 슬롯(1칸) / 테이블 등록 아이템(테이블 값) / 일반 아이템(기본값 n) 사거리를 계산하는 바인더
    /// </summary>
    [Serializable]
    public sealed class InventoryRangeBinder
    {
        private const int EMPTY_SLOT_RANGE = 1;

        [Header("Range Settings")]
        [Tooltip("ItemRangeTable에 등록되지 않은 일반 아이템 장착 시 기본 사거리(n)")]
        [SerializeField] private int _defaultItemRange = 2;

        private CharacterInventory _inventory;
        private TileDataProvider _tileDataProvider;

        private int _currentRange = EMPTY_SLOT_RANGE;
        private int _currentTargetCategoryMask = -1;
        private ulong _currentItemKey;
        private bool _isEmptySlot = true;

        public int CurrentRange => _currentRange;
        public int CurrentTargetCategoryMask => _currentTargetCategoryMask;
        public ulong CurrentItemKey => _currentItemKey;
        public bool IsEmptySlot => _isEmptySlot;

        public event Action<int, int, bool> OnRangeUpdated;

        public void Initialize(IActorHost host, TileDataProvider tileDataProvider)
        {
            _tileDataProvider = tileDataProvider;

            if (host != null && host.TryGetFeature(out CharacterDataFeature dataFeature))
            {
                _inventory = dataFeature.Inventory;
            }
        }

        public void Bind()
        {
            EventBus<CharacterInventory.SelectedSlotChangedEvent>.Subscribe(OnSelectedSlotChanged);
            RefreshCurrentSlot();
        }

        public void Unbind()
        {
            EventBus<CharacterInventory.SelectedSlotChangedEvent>.Unsubscribe(OnSelectedSlotChanged);
        }

        public void RefreshCurrentSlot()
        {
            if (_inventory == null)
            {
                ApplyEmptySlot();
                return;
            }

            EvaluateSlot(_inventory.CurItemIndex);
        }

        private void OnSelectedSlotChanged(CharacterInventory.SelectedSlotChangedEvent evt)
        {
            EvaluateSlot(evt.SlotIndex);
        }

        private void EvaluateSlot(int slotIndex)
        {
            if (_inventory == null)
            {
                ApplyEmptySlot();
                return;
            }

            ItemInstance slotItem = _inventory.GetItem(slotIndex);

            // 1. 빈 슬롯일 때: 무조건 1칸 사거리 적용
            if (slotItem == null || slotItem.IsEmpty() || slotItem.Record == null)
            {
                ApplyEmptySlot();
                return;
            }

            _isEmptySlot = false;
            _currentItemKey = slotItem.Record.HashedPK;

            // 2. ItemRangeTable에 등록된 아이템일 때: 테이블의 사거리 및 타겟 마스크 적용
            if (_tileDataProvider != null &&
                _tileDataProvider.TryGetItemRange(_currentItemKey, out ItemRangeRecord rangeRecord) &&
                rangeRecord != null)
            {
                _currentRange = Mathf.Max(1, rangeRecord.InteractionRange);
                _currentTargetCategoryMask = rangeRecord.TargetCategoryMask;
            }
            else
            {
                // 3. 테이블에 없는 일반 아이템일 때: 기본 사거리 n 적용
                _currentRange = Mathf.Max(1, _defaultItemRange);
                _currentTargetCategoryMask = 0;
            }

            OnRangeUpdated?.Invoke(_currentRange, _currentTargetCategoryMask, _isEmptySlot);
        }

        private void ApplyEmptySlot()
        {
            _isEmptySlot = true;
            _currentItemKey = 0UL;
            _currentRange = EMPTY_SLOT_RANGE;
            _currentTargetCategoryMask = -1;

            OnRangeUpdated?.Invoke(_currentRange, _currentTargetCategoryMask, _isEmptySlot);
        }
    }
}
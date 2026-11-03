using CoreEngine.Actor;
using CoreEngine.Extensions;
using CoreEngine.GameData;
using CoreEngine.Helpers;
using Farm.GameData.Item;
using System;
using System.Collections.Generic;
using UnityEngine;
// 타일 시스템 참조 추가
using CoreEngine.TileSystem.Logic;

namespace Farm.Character
{
    [System.Serializable]
    public class CharacterActionController : BaseActorFeature, IDisposable
    {
        // 현재 손에 들고 있는(선택된) 아이템 인스턴스
        private ItemInstance currentItem;
        private BaseCharacter character;
        private CharacterInventory inventory;

        // 타일 상호작용 FSM
        private TileInteractionFSM _tileFsm;
        // 캐릭터의 타일 타겟팅 정보를 가져오기 위한 체커
        private CharacterTileChecker _tileChecker;
        // 타일 작업 시 애니메이션 이펙트 인덱스 (기본값 설정 필요시 수정)
        private int _tileWorkAnimEffectIndex = 0;

        // 테이블에서 현재 아이템과 연결된 이펙트를 즉시 조회하기 위한 핸들러
        private static readonly TableHandler<ItemEffectImmediateTable, ItemEffectRecord> _immediateEffectTable = new();
        private static readonly TableHandler<ItemEffectOnAnimTable, ItemIndexedEffectRecord> _onAnimEffectTable = new();

        ~CharacterActionController()
        {
            Dispose();
        }

        public void Dispose()
        {
            // 해제할 이벤트/리소스가 있다면 여기서 정리
            if (_tileFsm != null)
            {
                _tileFsm.OnStateChanged -= HandleFsmStateChanged;
            }
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            character = Host as BaseCharacter;

            // 통합 데이터 핸들러(CharacterDataFeature)를 통해 인벤토리를 가져옵니다.
            if (Host.TryGetFeature(out CharacterDataFeature dataFeature))
            {
                inventory = dataFeature.Inventory;
            }
            else
            {
                LogHelper.LogWarning($"{nameof(CharacterDataFeature)}를 찾을 수 없어 인벤토리를 연결할 수 없습니다.");
            }

            // 타일 체커 가져오기
            Host.TryGetFeature(out _tileChecker);
        }

        /// <summary>
        /// 외부(GameManager 등)에서 코어 타일 FSM을 주입합니다.
        /// </summary>
        public void InjectTileFSM(TileInteractionFSM fsm)
        {
            if (_tileFsm != null)
            {
                _tileFsm.OnStateChanged -= HandleFsmStateChanged;
            }

            _tileFsm = fsm;

            if (_tileFsm != null)
            {
                _tileFsm.OnStateChanged += HandleFsmStateChanged;
            }
        }

        // FSM 상태 변화에 따른 애니메이션 트리거 등 처리
        private void HandleFsmStateChanged(TileFSMState state, Vector2Int targetCell)
        {
            switch (state)
            {
                case TileFSMState.Executing:
                    // TODO: 타일 작업 시작 애니메이션 재생 (예: anim.SetTrigger("Work"))
                    // 타일 작업 시작 시 1회성 이펙트가 필요하다면 여기서 호출할 수도 있습니다.
                    break;
                case TileFSMState.Idle:
                case TileFSMState.LockedOn:
                    // TODO: 작업 취소/완료 시 애니메이션 중단 로직 (필요시)
                    break;
            }
        }

        // 인벤토리에서 아이템을 선택/스크롤 했을 때 UI(또는 인벤토리) 측에서 호출
        public void EquipItem(int index)
        {
            if (inventory == null) return;

            ItemInstance curItem = inventory.GetItem(index);
            currentItem = curItem;

            if (curItem == null || curItem.IsEmpty())
            {
                // 빈 슬롯 선택됨
                // TODO: 플레이어 손의 무기 모델링 숨김 처리 등 필요한 경우 이곳에 작성
            }

            // 장비가 바뀌면 진행 중이던 타일 작업을 취소합니다.
            _tileFsm?.CancelInteraction();
        }

        // 마우스 클릭이나 아이템 사용 버튼을 눌렀을 때 호출 (즉시 발동형 이펙트 + 타일 작업 시작)
        public void OnItemUseInput()
        {
            if (currentItem == null || currentItem.IsEmpty()) return;

            // 1. 기존 즉시 발동 이펙트 처리
            if (_immediateEffectTable.TryGetReferencingRecords(ItemEffectImmediateTable.INDEX_EFFECT, currentItem.Record, out var effects))
            {
                for (int i = 0; i < effects.Count; i++)
                {
                    effects[i].ApplyEffect(character);
                }
            }

            // 2. 타일 작업 시작 지시 (FSM)
            // 현재 아이템이 타일 작업용 카테고리(도구 등)인지 판정하는 로직이 필요합니다.
            // 여기서는 임시로 _tileChecker가 유효한 타겟을 바라보고 있다면 작업 시작을 지시하도록 구성했습니다.
            if (_tileFsm != null && _tileChecker != null && inventory != null)
            {
                // 현재 마우스가 가리키는 셀을 타겟으로 잡습니다.
                // (주의: 마우스 커서 위치를 CharacterTileChecker에서 노출하는 프로퍼티(예: TargetCell)가 있다면 그것을 사용해야 합니다.)
                // 이 예제에서는 플레이어가 현재 서있는 셀을 테스트용 타겟으로 잡습니다.
                // Vector2Int targetCell = _tileChecker.TargetCell; // CharacterTileChecker에 이 프로퍼티 추가 필요
                Vector2Int targetCell = _tileChecker.CurrentCell;

                // 현재 아이템의 사거리/카테고리 마스크는 임시값입니다. 
                // (실제로는 ItemRecord나 ItemRangeTable에서 가져와야 함)
                byte layerIndex = 0; // 타일 레이어 인덱스
                int categoryMask = -1; // 임시 카테고리 마스크 (모두 허용)
                int maxRange = 1; // 임시 사거리

                _tileFsm.StartInteraction(targetCell, layerIndex, categoryMask, maxRange);
            }
        }

        /// <summary>
        /// 유니티 애니메이션 창에서 Event로 등록할 공통 함수 (타이밍 발동형 이펙트 + 타일 타격)
        /// </summary>
        public void OnAnimEventTrigger(int effectIndex)
        {
            // 1. 타일 타격 (FSM)
            if (_tileFsm != null && _tileFsm.CurrentState == TileFSMState.Executing)
            {
                // 설정된 타일 작업 애니메이션 이펙트 인덱스와 일치할 때만 타격을 인정
                if (effectIndex == _tileWorkAnimEffectIndex)
                {
                    // 현재 아이템의 타일 공격력을 가져오는 로직 필요
                    int workAmount = 1; // 임시 타일 피해량
                    _tileFsm.ApplyWorkTick(workAmount);
                }
            }

            // 2. 기존 애니메이션 이펙트 처리
            if (!TryGetOnAnimEffects(out var animEffects)) return;

            foreach (var effect in animEffects)
            {
                if (effect.Index == effectIndex)
                {
                    effect.ApplyEffect(character);
                    break;
                }
            }
        }

        /// <summary>
        /// 모든 AnimEvent 호출
        /// </summary>
        public void OnAnimEventTrigger()
        {
            // 타일 타격 (기본적으로 첫 번째 이펙트를 타일 타격으로 간주)
            if (_tileFsm != null && _tileFsm.CurrentState == TileFSMState.Executing)
            {
                int workAmount = 1; // 임시 타일 피해량
                _tileFsm.ApplyWorkTick(workAmount);
            }

            if (!TryGetOnAnimEffects(out var animEffects)) return;
            foreach (var effect in animEffects) effect.ApplyEffect(character);
        }

        private bool TryGetOnAnimEffects(out IReadOnlyList<ItemIndexedEffectRecord> animEffects)
        {
            animEffects = null;
            if (currentItem == null || currentItem.IsEmpty())
            {
                LogHelper.LogWarning("현재 선택된 아이템이 없음");
                return false;
            }

            if (_onAnimEffectTable.TryGetReferencingRecords(ItemEffectOnAnimTable.INDEX_EFFECT, currentItem.Record, out var effects))
            {
                animEffects = effects;
                return true;
            }
            return false;
        }
    }
}
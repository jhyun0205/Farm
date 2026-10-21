using CoreEngine.Actor;
using CoreEngine.Extensions;
using CoreEngine.GameData;
using CoreEngine.Helpers;
using Farm.GameData.Item;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Farm.Character
{
    [System.Serializable]
    public class CharacterActionController : BaseActorFeature, IDisposable
    {
        // 현재 손에 들고 있는(선택된) 아이템 인스턴스
        private ItemInstance currentItem;
        private BaseCharacter character;
        private CharacterInventory inventory;

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
        }

        // 마우스 클릭이나 아이템 사용 버튼을 눌렀을 때 호출 (즉시 발동형 이펙트)
        public void OnItemUseInput()
        {
            if (currentItem == null || currentItem.IsEmpty()) return;

            // ItemEffectTable에서 현재 들고 있는 아이템을 참조하는 모든 효과를 조회
            if (_onAnimEffectTable.TryGetReferencingRecords(ItemEffectImmediateTable.INDEX_EFFECT, currentItem.Record, out var effects))
            {
                for (int i = 0; i < effects.Count; i++)
                {
                    effects[i].ApplyEffect(character);
                }
            }
        }

        /// <summary>
        /// 유니티 애니메이션 창에서 Event로 등록할 공통 함수 (타이밍 발동형 이펙트)
        /// </summary>
        public void OnAnimEventTrigger(int effectIndex)
        {
            if (!TryGetOnAnimEffects(out var animEffects)) return;

            foreach(var effect in animEffects)
            {
                if(effect.Index == effectIndex)
                {
                    effect.ApplyEffect(character);
                    break;
                }
            }

            //if (0 <= effectIndex && effectIndex < animEffects.Count)
            //{
            //    animEffects[effectIndex].ApplyEffect(character);
            //}
            //else
            //{
            //    LogHelper.LogError($"존재하지 않는 AnimeEvent 인덱스: {effectIndex} (아이템: {currentItem.Record.Name})");
            //}
        }

        /// <summary>
        /// 모든 AnimEvent 호출
        /// </summary>
        public void OnAnimEventTrigger()
        {
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
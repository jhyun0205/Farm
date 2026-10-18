using CoreEngine;
using CoreEngine.EventBus;
using CoreEngine.Facades;
using CoreEngine.Helpers;
using CoreEngine.Input;
using CoreEngine.Interface;
using CoreEngine.Ui;
using Farm.Character;
using Farm.Character.Move;
using Farm.Input;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Farm.Controller
{
    public struct ControlTargetChangedEvent : IEvent
    {
        public readonly PlayableCharacter ControlTarget;
        public ControlTargetChangedEvent(PlayableCharacter controlTarget)
        {
            ControlTarget = controlTarget;
        }
    }

    public interface QuickSlotControl
    {
        public void HandleUI_QuickSlotClicked(int index);
    }

    public class GameController : BaseActor, ITickable, QuickSlotControl
    {
        private PlayableCharacter controlTarget;

        // [수정됨] 통합 데이터 핸들러 묶음(CharacterDataFeature)으로 캐싱
        private CharacterDataFeature characterData;
        private CharacterMoveFeature move;

        // [수정됨] characterData가 null일 수 있으므로 null 조건부 연산자(?) 적용
        private CharacterInventory Inventory => characterData?.Inventory;

        public TickGroup TickGroup => TickGroup.Controller;

        private readonly InterfaceBinderContainer _interfaceBinder = new();
        private readonly InterfaceReceiver<IMoveInput> _moveInputReceiver = new();
        private readonly InterfaceReceiver<ISprintInput> _sprintInputReceiver = new();
        private readonly InterfaceReceiver<IScrollDeltaInput> _scrollDeltaInputReceiver = new();

        private void Awake()
        {
            _interfaceBinder.Add(_moveInputReceiver);
            _interfaceBinder.Add(_sprintInputReceiver);
            _interfaceBinder.Add(_scrollDeltaInputReceiver);
            _interfaceBinder.Add(new InterfacePublisher<QuickSlotControl>(this));
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            _interfaceBinder.BindAll();

            EventBus<UseItemEvent>.Subscribe(OnUseItem);
            EventBus<CharacterRequestEvent>.Subscribe(OnCharacterChanged);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            _interfaceBinder.UnbindAll();

            EventBus<UseItemEvent>.Unsubscribe(OnUseItem);
            EventBus<CharacterRequestEvent>.Unsubscribe(OnCharacterChanged);
        }

        public void Tick(float deltaTime)
        {
            if (controlTarget == null) return;
            InputMove();
            InputSprint();
            InputScroll();
        }

        #region Tick
        private void InputMove()
        {
            if (!_moveInputReceiver.TryGet(out var moveInput)) return;
            move?.Move(moveInput.Value);
        }

        private void InputSprint()
        {
            if (!_sprintInputReceiver.TryGet(out var sprintInput)) return;
            if (move != null && move.IsSprint != sprintInput.Value)
            {
                move.SetSprint(sprintInput.Value);
            }
        }

        private void InputScroll()
        {
            if (!_scrollDeltaInputReceiver.TryGet(out var scrollDeltaInput)) return;

            if (scrollDeltaInput.Value != 0)
            {
                Inventory?.ScrollSlot(scrollDeltaInput.Value);
            }
        }
        #endregion

        #region Handle Ui
        public void HandleUI_QuickSlotClicked(int index)
        {
            // Controller가 UI 이벤트를 받아 Model에게 데이터 변경을 지시
            Inventory?.SetSelectedSlot(index);
        }
        #endregion

        public void OnUseItem(UseItemEvent evt)
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                LogHelper.Log("UI 클릭 중이므로 아이템 사용 액션을 무시");
                return;
            }
            Inventory?.UseItem();
        }

        #region Character Request Process
        private void OnCharacterChanged(CharacterRequestEvent evt)
        {
            switch (evt.Request)
            {
                case CharacterRequest.RequestControl:
                    OnRequestControl(evt.requester);
                    break;

                case CharacterRequest.RequestRelease:
                    OnRequestRelease(evt.requester);
                    break;
            }
        }

        private void OnRequestControl(PlayableCharacter requester)
        {
            if (requester == null) return;

            // 컨트롤할 대상이 있는 경우 탈출
            if (controlTarget != null)
            {
                LogHelper.LogWarning("이미 컨트롤할 캐릭터가 존재함");
                return;
            }

            // [수정됨] Inventory Feature를 직접 가져오지 않고 통합 데이터 매니저인 CharacterDataFeature를 가져옴
            if (!requester.TryGetFeature(out characterData))
            {
                LogHelper.LogWarning($"{requester.name}에서 CharacterDataFeature를 찾을 수 없습니다.");
            }

            if (!requester.TryGetFeature(out move))
            {
                LogHelper.LogWarning($"{requester.name}에서 CharacterMoveFeature를 찾을 수 없습니다.");
            }

            controlTarget = requester;
            PublishControlTargetChanged();
        }

        private void OnRequestRelease(PlayableCharacter requester)
        {
            // Despawn된 캐릭터가 내가 담당하던 캐릭터면 컨트롤에서 놓아줌
            if (controlTarget == requester)
            {
                controlTarget = null;
                characterData = null; // [추가됨] 이전 캐릭터의 데이터 참조 해제
                move = null;          // [추가됨] 이전 캐릭터의 이동 로직 참조 해제
                PublishControlTargetChanged();
            }
        }

        private void PublishControlTargetChanged()
        {
            var evt = new ControlTargetChangedEvent(controlTarget);
            EventBus<ControlTargetChangedEvent>.Publish(evt);
        }
        #endregion
    }
}
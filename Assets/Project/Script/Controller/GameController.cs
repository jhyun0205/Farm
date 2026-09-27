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
    public struct ControlTargetChangedEvent :IEvent
    {
        public readonly PlayableCharacter ControlTarget;
        public ControlTargetChangedEvent(PlayableCharacter controlTarget )
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
        //private UserInputManager inputManager;
        //private UiController uiController;

        private PlayableCharacter controlTarget;
        private CharacterInventory inventory;
        private CharacterMoveFeature move;

        // 각 객체는(특히 Ui) 어떤 캐릭터가 대상이 될지 모르니 캐릭터의 이벤트에 연결할 수 없음
        // 그러므로 컨트롤러에서 해결
        //public event System.Action<PlayableCharacter> Event_OnControllTargetSet;
        //public event System.Action<PlayableCharacter> Event_OnControllTargetRemoved;

        public TickGroup TickGroup => TickGroup.Controller;
        //public static event System.Action<float/*마우스 휠 스크롤*/> OnQuickSlotScrollInput;

        private readonly InterfaceBinderContainer _interfaceBinder = new();
        private readonly InterfaceReceiver<IMoveInput> _moveInputReceiver = new();
        private readonly InterfaceReceiver<ISprintInput> _sprintInputReceiver = new();
        private readonly InterfaceReceiver<IScrollDeltaInput> _scrollDeltaInputReceiver= new();

        private void Awake()
        {
            //inputManager = CoreFacade.GetManager<UserInputManager>();

            //Debug.Log("uiController 안씀");
            //uiController = null; // WorldManager.GetObject<UiController>();


            _interfaceBinder.Add(_moveInputReceiver);
            _interfaceBinder.Add(_sprintInputReceiver);
            _interfaceBinder.Add(_scrollDeltaInputReceiver);
            _interfaceBinder.Add(new InterfacePublisher<QuickSlotControl>(this));
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            _interfaceBinder.BindAll();

            //inputManager.Event_OnUseItemInput += UseItem;
            EventBus<UseItemEvent>.Subscribe(OnUseItem);
            EventBus<CharacterRequestEvent>.Subscribe(OnCharacterChanged);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            _interfaceBinder.UnbindAll();

            //inputManager.Event_OnUseItemInput -= UseItem;
            EventBus<UseItemEvent>.Unsubscribe(OnUseItem);
            EventBus<CharacterRequestEvent>.Unsubscribe(OnCharacterChanged);
        }





        //public void SetControllTarget(PlayableCharacter newCharacter)
        //{
        //    if (newCharacter != null)
        //    {
        //        // 기존 캐릭터가 있으면 메모리 정리
        //        if (controlTarget)
        //        {
        //            controlTarget.OnControllTargetRemoved();
        //            Event_OnControllTargetRemoved?.Invoke(controlTarget);
        //        }
        //        controlTarget = newCharacter;
        //        if (!controlTarget.TryGetFeature(out inventory)) return;

        //        // 새캐릭터에 이벤트 연결됐음을 알림
        //        controlTarget.OnControllTargetSet();
        //        Event_OnControllTargetSet?.Invoke(newCharacter);
        //    }
        //    else
        //    {
        //        Debug.LogWarning("컨트롤 타겟이 존재하지 않음");
        //    }
        //}

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
            move.Move(moveInput.Value);
            //character?.Move(inputManager.Move);
        }

        private void InputSprint()
        {
            if (!_sprintInputReceiver.TryGet(out var sprintInput)) return;
            if(move.IsSprint != sprintInput.Value)
            {
                move.SetSprint(sprintInput.Value);
            }
            //character?.SprintHold(inputManager.Sprint);
        }

        private void InputScroll()
        {
            if (!_scrollDeltaInputReceiver.TryGet(out var scrollDeltaInput)) return;
            
            if (scrollDeltaInput.Value != 0)
            {
                inventory?.ScrollSlot(scrollDeltaInput.Value);
            }
        }
        #endregion


        #region Handle Ui
        public void HandleUI_QuickSlotClicked(int index)
        {
            // Controller가 UI 이벤트를 받아 Model에게 데이터 변경을 지시
            inventory?.SetSelectedSlot(index);
        }
        #endregion

        public void OnUseItem(UseItemEvent evt)
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                LogHelper.Log("UI 클릭 중이므로 아이템 사용 액션을 무시");
                return;
            }
            inventory?.UseItem();
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

            requester.TryGetFeature(out inventory);
            requester.TryGetFeature(out move);
            controlTarget = requester;
            PublishControlTargetChanged();
        }

        private void OnRequestRelease(PlayableCharacter requester)
        {
            // Despawn된 캐릭터가 내가 담당하던 캐릭터면 컨트롤에서 놓아줌
            if (controlTarget == requester)
            {
                controlTarget = null;
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


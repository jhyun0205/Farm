using CoreEngine;
using CoreEngine.EventBus;
using CoreEngine.Helpers;
using CoreEngine.Interface;
using Farm.Character;
using Farm.Controller;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Farm.UI.Item
{
    public interface IQuickSlotUpdate
    {
        /// <summary>
        /// 새로 선택된 슬롯번호로 View 업데이트
        /// </summary>
        /// <param name="newIndex"></param>
        public void OnSelectedSlotChanged(int newIndex);

        /// <summary>
        /// 아이템View에 새로운 아이템데이터 적용
        /// </summary>
        public void OnItemSlotChanged(int index);
    }
    public class QuickSlot : CoreMonoBehaviour, IQuickSlotUpdate
    {
        private BaseButton[] itemSlots;
        private ItemViewer[] itemViewers;

        // [추가] UI에서 현재 선택된 슬롯 번호를 기억하기 위한 변수
        private int currentIndex = -1;

        private readonly InterfaceBinderContainer _binder = new();
        private readonly InterfaceReceiver<QuickSlotControl> _controllerReceiver = new();

        protected void Awake()
        {
            _binder.Add(_controllerReceiver);
            _binder.Add(new InterfacePublisher<IQuickSlotUpdate>(this));
            _binder.BindAll();

            itemSlots = GetComponentsInChildren<BaseButton>();
            itemViewers = new ItemViewer[itemSlots.Length];

            for (int i = 0; i < itemSlots.Length; i++)
            {
                int index = i; // callback 등록을 위해 변수 생성

                // 같은 오브젝트에 붙은 뷰어 가져오기
                itemViewers[i] = itemSlots[i].GetComponent<ItemViewer>();

                itemSlots[i].Initialize();

                // 버튼 클릭 시 로컬 함수 호출
                itemSlots[i].AddCallback(() => OnSlotClicked(index));
            }
            EventBus<ControlTargetChangedEvent>.Subscribe(OnControlTargetChanged);
            //controller.Event_OnControllTargetSet += OnControllTargetSet;
            //controller.Event_OnControllTargetRemoved += OnControllTargetRemoved;
        }

        private void OnDestroy()
        {
            _binder.UnbindAll();
            // 이벤트 정리
            for (int i = 0; i < itemSlots.Length; i++)
            {
                itemSlots[i].ClearCallback();
            }
            EventBus<ControlTargetChangedEvent>.Unsubscribe(OnControlTargetChanged);
            //if (controller != null)
            //{
            //    controller.Event_OnControllTargetSet -= OnControllTargetSet;
            //    controller.Event_OnControllTargetRemoved -= OnControllTargetRemoved;
            //}
        }

        private void OnControlTargetChanged(ControlTargetChangedEvent evt)
        {
            if (evt.ControlTarget != null)
            {
                OnControllTargetSet(evt.ControlTarget);
            }
        }

        private void OnSlotClicked(int index)
        {
            // [추가] 이미 선택된 버튼을 또 눌렀다면 아무 작업도 하지 않고 무시
            if (index == currentIndex)
            {
                return;
            }

            // V -> C [O]
            if (!_controllerReceiver.TryGet(out var controller)) return;
            controller.HandleUI_QuickSlotClicked(index);
        }

        // 컨트롤 타겟이 바뀔때마다 이벤트가 연결되는 대상 교체
        private void OnControllTargetSet(PlayableCharacter newCharacter)
        {
            if (!newCharacter.TryGetFeature(out CharacterInventory inventory)) return;

            //inventory.Event_OnSelectedSlotChanged += OnSelectedSlotChanged;
            //inventory.Event_OnItemSlotChanged += OnItemSlotChanged;

            // view에 model을 연결
            for (int i = 0; i < itemViewers.Length; i++)
            {
                ItemViewer viewer = itemViewers[i];
                viewer.Connect(inventory.Items[i]);
                viewer.UpdateView();
            }

            OnSlotClicked(0); // 기본 선택
        }

        //private void OnControllTargetRemoved(PlayableCharacter oldCharacter)
        //{
        //    CharacterInventory inventory = null;
        //    if (!oldCharacter.TryGetFeature(out inventory)) return;

        //    inventory.Event_OnSelectedSlotChanged -= OnSelectedSlotChanged;
        //    inventory.Event_OnItemSlotChanged -= OnItemSlotChanged;
        //}


        // Model에서 데이터가 변경되었을 때 화면만 그리는 역할
        //public void OnSelectedSlotChanged(int newIndex)
        //{
        //    if (itemSlots == null || itemSlots.Length == 0) return;


        //    if (0 <= currentIndex && currentIndex < itemSlots.Length)
        //    {
        //        itemSlots[currentIndex].SetInteractable(true);
        //    }
        //    // 새로 변경된 인덱스를 UI 캐시 변수에 저장
        //    currentIndex = newIndex;

        //    GameObject targetSlot = itemSlots[currentIndex].gameObject;
        //    itemSlots[currentIndex].SetInteractable(false);
        //    EventSystem.current.SetSelectedGameObject(targetSlot);
        //}

        void IQuickSlotUpdate.OnSelectedSlotChanged(int newIndex)
        {
            if (itemSlots == null || itemSlots.Length == 0) return;


            if (0 <= currentIndex && currentIndex < itemSlots.Length)
            {
                itemSlots[currentIndex].SetInteractable(true);
            }
            // 새로 변경된 인덱스를 UI 캐시 변수에 저장
            currentIndex = newIndex;

            GameObject targetSlot = itemSlots[currentIndex].gameObject;
            itemSlots[currentIndex].SetInteractable(false);
            if(EventSystem.current == null)
            {
                LogHelper.LogWarning("EventSystem.current가 아직 준비되지 않음");
                return;
            }
            EventSystem.current.SetSelectedGameObject(targetSlot);
        }



        //private void OnItemSlotChanged(int index)
        //{
        //    itemViewers[index].UpdateView();
        //}

        void IQuickSlotUpdate.OnItemSlotChanged(int index)
        {
            itemViewers[index].UpdateView();
        }
    }
}

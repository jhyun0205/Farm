using CoreEngine;
using CoreEngine.EventBus;
using Farm.Character;
using Farm.GameData.Item;
using UnityEngine;
using UnityEngine.UI;

namespace Farm.UI.Item
{
    public class BagSlotController : CoreMonoBehaviour
    {
        public struct BagPopupToggleEvent : IEvent { }   // 가방 팝업한테 "열고 닫아줘"라고 알리는 용도

        [Header("테스트용 (실기능 연결 전까지 수동 확인용)")]
        [SerializeField] private bool debugHasBag;              // 인스펙터에서 체크/해제하며 테스트

        private const float InactiveAlpha = 0.3f;   // 비활성 상태일 때 흐릿하게 보일 투명도

        private Image iconImage;   // 자식 "ItemIcon"의 Image, 직접 찾아옴
        private BaseButton button;
        private bool isActivated;
        private bool lastDebugHasBag;   // 인스펙터 값 변화 감지용

        private void Awake()
        {
            iconImage = transform.Find("ItemIcon").GetComponent<Image>();   // 수동 드래그 대신 자식 이름으로 직접 참조
            button = GetComponent<BaseButton>();
            button.Initialize();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            SetActivated(debugHasBag);   // 시작 상태를 테스트 토글 값에 맞춤
            lastDebugHasBag = debugHasBag;
            EventBus<CharacterInventory.ItemAcquiredEvent>.Subscribe(OnItemAcquired);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            EventBus<CharacterInventory.ItemAcquiredEvent>.Unsubscribe(OnItemAcquired);
        }

        private void Update()   // 테스트용 - 인스펙터에서 체크박스 바꾸면 바로 반영
        {
            if (debugHasBag != lastDebugHasBag)
            {
                lastDebugHasBag = debugHasBag;
                SetActivated(debugHasBag);
            }
        }

        private void OnItemAcquired(CharacterInventory.ItemAcquiredEvent evt)
        {
            if (isActivated) return;
            if (evt.ItemKey != ItemTableKeys._1_가방) return;

            SetActivated(true);
        }

        private void SetActivated(bool activated)
        {
            isActivated = activated;

            if (iconImage != null)
            {
                Color c = Color.white;
                c.a = activated ? 1f : InactiveAlpha;
                iconImage.color = c;
            }

            button.SetInteractable(activated);

            if (activated)
            {
                button.SetCallback(OnBagClicked);
            }
        }

        private void OnBagClicked()
        {
            EventBus<BagPopupToggleEvent>.Publish(new BagPopupToggleEvent());   // 팝업 쪽에 직접 참조 없이 이벤트로만 요청
        }
    }
}

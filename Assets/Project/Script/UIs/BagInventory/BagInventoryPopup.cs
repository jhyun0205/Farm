using CoreEngine.EventBus;
using Farm.UI.Item;
using UnityEngine;

namespace Farm.UI
{
    public class BagInventoryPopup : MonoBehaviour   // 가방 팝업 자신을 열고 닫는 역할만 담당
    {
        private bool isOpen;

        private void Awake()
        {
            // 루트 자신은 항상 켜둔 채로 자식들만 껐다 켰다 함
            // (루트를 꺼버리면 Awake가 다시 안 돌아서 토글 요청을 영영 못 받게 됨)
            SetChildrenActive(false);

            // OnEnable/OnDisable이 아니라 Awake/OnDestroy를 쓰는 이유:
            // 구독을 OnEnable/OnDisable에 두면 꺼질 때마다 해제돼서 다시 못 받게 됨
            EventBus<BagSlotController.BagPopupToggleEvent>.Subscribe(OnToggleRequested);
        }

        private void OnDestroy()
        {
            EventBus<BagSlotController.BagPopupToggleEvent>.Unsubscribe(OnToggleRequested);
        }

        private void OnToggleRequested(BagSlotController.BagPopupToggleEvent evt)
        {
            isOpen = !isOpen;
            SetChildrenActive(isOpen);
        }

        private void SetChildrenActive(bool active)
        {
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(active);
            }
        }
    }
}

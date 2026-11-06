using CoreEngine;
using CoreEngine.EventBus;
using Farm.Character;
using Farm.Controller;
using UnityEngine;
using UnityEngine.UI;

namespace Farm.UI
{
    public class FatigueBarUI : CoreMonoBehaviour // 화면 고정 HUD용, World Space 아님
    {
        private Image barImage; // Image Type = Filled, Fill Method = Horizontal로 설정해둘 것 — Awake에서 자동으로 찾음

        private CharacterFatigueFeature fatigueFeature; // 구독 해제를 위해 보관

        private void Awake()
        {
            barImage = GetComponent<Image>(); // 이 오브젝트에 붙어있는 Image를 자동으로 가져옴 (Inspector 드래그 불필요)
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            EventBus<ControlTargetChangedEvent>.Subscribe(OnControlTargetChanged); // 조작 캐릭터가 바뀔 때마다 알림 받기
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            EventBus<ControlTargetChangedEvent>.Unsubscribe(OnControlTargetChanged);
            UnsubscribeFatigue(); // 지금 들고 있던 구독도 정리
        }

        private void OnControlTargetChanged(ControlTargetChangedEvent evt)
        {
            UnsubscribeFatigue(); // 이전 캐릭터 구독 해제

            if (evt.ControlTarget != null && evt.ControlTarget.TryGetFeature(out fatigueFeature))
            {
                UpdateBar(fatigueFeature.CurFatigue); // 바뀐 직후 현재값 1회 반영
                fatigueFeature.OnFatigueChanged += UpdateBar;
            }
        }

        private void UnsubscribeFatigue()
        {
            if (fatigueFeature != null)
            {
                fatigueFeature.OnFatigueChanged -= UpdateBar;
                fatigueFeature = null;
            }
        }

        private void UpdateBar(int curFatigue)
        {
            barImage.fillAmount = fatigueFeature.FatigueRatio; // 0~1 비율 그대로 적용
        }
    }
}

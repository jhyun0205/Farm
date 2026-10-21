using CoreEngine;
using CoreEngine.EventBus;
using Farm.Input;
using System;
using UnityEngine;

namespace Farm.Fishing
{
    public class FishingMiniGame : CoreMonoBehaviour, ITickable
    {
        [Header("UI 참조")]
        [SerializeField] private RectTransform bar; //전체 막대기
        [SerializeField] private RectTransform targetBox; // 목표존 박스
        [SerializeField] private RectTransform arrow;   // 화살표

        [Header("목표존 설정 ")]
        [SerializeField] private float targetBoxWidthRatio = 0.2f;
        [SerializeField] private float targetBoxSpeed = 0.2f;

        [Header("화살표 설정")]
        [SerializeField] private float arrowSpeed = 1.5f;

        private float barHalfWidth;
        private float targetBoxRange;
        private float arrowRange;

        public TickGroup TickGroup => TickGroup.Ui;

        private void Awake()
        {
            barHalfWidth = bar.rect.width / 2f;

            Vector2 size = targetBox.sizeDelta;
            size.x = bar.rect.width * targetBoxWidthRatio;
            targetBox.sizeDelta = size;

            targetBoxRange = barHalfWidth - (targetBox.sizeDelta.x / 2f);
            arrowRange = barHalfWidth - (arrow.sizeDelta.x / 2f);
        }

        public void Tick(float deltaTime)
        {
            MoveTargetBox();
            MoveArrow();
        }

        private void MoveTargetBox()
        {
            float t = Mathf.PingPong(Time.time * targetBoxSpeed, 2f) - 1f;
            targetBox.anchoredPosition = new Vector2(t * targetBoxRange, targetBox.anchoredPosition.y);
        }

        private void MoveArrow()
        {
            float t = Mathf.PingPong(Time.time * arrowSpeed, 2f) - 1;
            arrow.anchoredPosition = new Vector2(t * arrowRange, arrow.anchoredPosition.y);
        }
    }
}

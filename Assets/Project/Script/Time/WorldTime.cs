using CoreEngine;
using CoreEngine.EventBus;
using Farm.GameLogic.Time;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Farm.UI
{
    public class WorldTime : CoreMonoBehaviour
    {
        private Sprite[] handSprites;   // Resources에서 직접 로드, 인스펙터 연결 불필요

        private Image handImage;   // Image 출력
        private int currentHour;   // 0~23 시간 순환

        private void Awake()
        {
            handImage = transform.Find("ClockHand").GetComponent<Image>();   // 자식 이름으로 직접 참조

            // Resources 폴더 기준 상대경로, 확장자 제외
            Sprite[] loaded = Resources.LoadAll<Sprite>("ClockHand/clock hand");

            // LoadAll은 로드 순서를 보장하지 않으므로 이름 끝 숫자(_0~_7) 기준으로 직접 정렬
            handSprites = loaded.OrderBy(s => int.Parse(s.name.Split('_').Last())).ToArray();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            EventBus<GameHourPassedEvent>.Subscribe(OnHourPassed);   // 시간 지날 때마다 알림 받기 시작
            UpdateHandSprite();   // 켜지는 즉시 현재 시간 기준으로 바늘 한번 맞춰줌
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            EventBus<GameHourPassedEvent>.Unsubscribe(OnHourPassed);
        }

        private void OnHourPassed(GameHourPassedEvent evt)
        {
            currentHour = (currentHour + evt.DeltaHours) % 24;   // 지나간 시간만큼 더하고 24시간으로 순환
            UpdateHandSprite();   // 누적된 시간에 맞춰 바늘 갱신
        }

        private void UpdateHandSprite()
        {
            int index = currentHour / 3;   // 3시간당 한 칸이므로 0~7 중 하나로 변환
            handImage.sprite = handSprites[index];   // 해당 인덱스의 미리 잘린 바늘 스프라이트로 교체
        }
    }
}

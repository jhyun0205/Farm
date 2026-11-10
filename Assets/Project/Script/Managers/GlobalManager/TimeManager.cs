using CoreEngine.EventBus;
using CoreEngine;
using CoreEngine.TimeSystem;  // TimeDirector가 위치한 네임스페이스
using System;
using System.Collections;
using UnityEngine;

namespace Farm.GameLogic.Time
{
    public struct GameMinutePassedEvent : IEvent
    {
        public int DeltaMinutes;
    }

    public struct GameHourPassedEvent : IEvent
    {
        public int DeltaHours;
    }

    public class TimeManager : BaseManager, ITickable
    {
        [Header("기획 수치 설정")]
        [Tooltip("현실의 1초 = 게임 속 1분")]
        private const float REAL_SECONDS_PER_GAME_MINUTE = 1f;

        [Tooltip("현실의 60초 = 게임 속 1시간")]
        private const float REAL_SECONDS_PER_GAME_HOUR = 3f; // [시계 테스트용] 원래 60f

        public static float DayLength { get; private set; } = 24f * 60f;
        public double TotalGameSeconds { get; private set; }

        // UpdateDirector가 이 객체를 언제 실행할지 지정
        public TickGroup TickGroup => TickGroup.Controller;

        private float _timerForMinute = 0f;
        private float _timerForHour = 0f;

        protected override IEnumerator OnInitialize()
        {
            yield return base.OnInitialize();
            // BaseManager(BaseLeaf)의 OnEnable에서 RegisterTick()이 자동 호출되므로 초기화만 수행
            _timerForMinute = 0f;
            _timerForHour = 0f;
        }

        /// <summary>
        /// DataManager(우선순위 0순위)가 세이브 파일을 읽은 후, 이 메서드를 호출하여 데이터를 주입(Inject)합니다.
        /// </summary>
        public void LoadTimeData(double savedTotalTime, DateTime lastSaveTime)
        {
            TotalGameSeconds = savedTotalTime;

            // TimeDirector의 UTC 유틸리티를 활용하거나 직접 오프라인 경과 시간 계산
            float offlineSeconds = (float)(DateTime.UtcNow - lastSaveTime).TotalSeconds;
            AdvanceTime(offlineSeconds);
        }

        public void OnPlayerSleep()
        {
            // 침대 취침 시 8시간 강제 경과 처리
            AdvanceTime(8f * REAL_SECONDS_PER_GAME_HOUR);
        }

        /// <summary>
        /// 강제로 시간을 흐르게 하거나, 매 프레임 업데이트 시 공통으로 사용하는 시간 연산 로직
        /// </summary>
        public void AdvanceTime(float secondsToAdd)
        {
            TotalGameSeconds += secondsToAdd;
            _timerForMinute += secondsToAdd;
            _timerForHour += secondsToAdd;

            if (_timerForMinute >= REAL_SECONDS_PER_GAME_MINUTE)
            {
                int deltaMins = (int)(_timerForMinute / REAL_SECONDS_PER_GAME_MINUTE);
                _timerForMinute -= deltaMins * REAL_SECONDS_PER_GAME_MINUTE;

                EventBus<GameMinutePassedEvent>.Publish(new GameMinutePassedEvent { DeltaMinutes = deltaMins });
            }

            if (_timerForHour >= REAL_SECONDS_PER_GAME_HOUR)
            {
                int deltaHours = (int)(_timerForHour / REAL_SECONDS_PER_GAME_HOUR);
                _timerForHour -= deltaHours * REAL_SECONDS_PER_GAME_HOUR;

                EventBus<GameHourPassedEvent>.Publish(new GameHourPassedEvent { DeltaHours = deltaHours });
            }
        }

        // --- UpdateDirector에 의해 매 프레임 호출 ---
        public void Tick(float deltaTime)
        {
            // UpdateDirector가 넘겨주는 순수 deltaTime 대신, 
            // 1계층 TimeDirector가 가공한(일시정지, 배속 등이 적용된) World 시계를 참조합니다.
            AdvanceTime(TimeDirector.Inst.WorldDeltaTime);
        }

        public int GetCurrentDay() => (int)(TotalGameSeconds / DayLength) + 1;

        public void SetWorldTime(float dayLength)
        {
            DayLength = dayLength;
        }
    }
}
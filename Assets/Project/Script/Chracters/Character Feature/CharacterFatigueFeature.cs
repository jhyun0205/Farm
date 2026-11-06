using CoreEngine.Actor;
using CoreEngine.EventBus;
using System;
using UnityEngine;

namespace Farm.Character
{
    public struct FatigueGainEvent : IEvent 
    {
        public readonly int amount;
        public FatigueGainEvent(int amount)
        {
            this.amount = amount;
        }
    }

    [System.Serializable]
    public class CharacterFatigueFeature : BaseActorFeature, IDisposable
    {
        private const int MaxFatigue = 40; // 피로도 최대치

        private int curFatigue; // 현재 피로도 값

        public int CurFatigue => curFatigue; // 현재 피로도 원본값
        public int MaxFatigueValue => MaxFatigue; // UI가 참조할 최대치
        public float FatigueRatio => (float)curFatigue / MaxFatigue; // UI에 넘길 0~1 비율
        public bool IsFatigueMax => curFatigue >= MaxFatigue; // 최대치 도달 여부 (나중에 행동 차단할 때 씀)

        public event Action<int> OnFatigueChanged; // 값이 바뀔 때마다 현재 피로도를 방송

        protected override void OnInitialized()
        {
            base.OnInitialized();
            curFatigue = 0; // 시작은 0부터 (스테미너랑 반대로 "쌓이는" 개념이라)
            OnFatigueChanged?.Invoke(curFatigue);
            EventBus<FatigueGainEvent>.Subscribe(OnFatigueGainRequested); // 어디서든 이벤트만 던지면 여기서 받아서 처리
        }

        public void Dispose()
        {
            EventBus<FatigueGainEvent>.Unsubscribe(OnFatigueGainRequested);
        }

        private void OnFatigueGainRequested(FatigueGainEvent evt)
        {
            AddFatigue(evt.amount);
        }

        public void AddFatigue(int amount = 1) // 직접 호출할 수도 있지만, 보통은 이벤트를 통해서 들어옴
        {
            if (curFatigue >= MaxFatigue) return; // 이미 꽉 찼으면 더 안 쌓임

            curFatigue = Mathf.Min(curFatigue + amount, MaxFatigue);
            OnFatigueChanged?.Invoke(curFatigue);
        }
    }
}
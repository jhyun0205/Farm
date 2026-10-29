using CoreEngine; 
using CoreEngine.Actor; 
using Farm.Character.Move;
using System;
using UnityEngine;

namespace Farm.Character 
{
    [System.Serializable] 
    public class CharacterStaminaFeature : BaseActorFeature, ITick 
    {
        [SerializeField] private int staminaCount = 1; // 보유한 그릇 개수 (기본 1개부터 시작)
        [SerializeField] private float drainPerSecond = 10f; // 초당 소모량 (스프린트 중)
        [SerializeField] private float regenPerSecond = 15f; // 초당 회복량 (스프린트 멈췄을 때)
        [SerializeField] private float staminaPerSegment = 60f; //stamina 1칸이 담당하는 스테미너 양

        private float curStamina; // 현재 스테미너 값
        private CharacterMoveFeature moveFeature; // 이동/스프린트 상태를 확인하기 위한 참조

        public float StaminaRatio => curStamina / MaxStamina; // UI에 넘겨줄 0~1 비율 값
        public float CurStamina => curStamina; //현재 스테미너 원본값 (조각별 계산에 필요)
        public float MaxStamina => staminaPerSegment * staminaCount; // 그릇 1개당 용량 × 그릇 개수로 계산
        public float StaminaPerSegment => staminaPerSegment; //  조각 1칸당 용량
        public int SegmentCount => staminaCount; // 그릇 개수 그대로 노출 (UI가 몇 개 그릴지 결정)

        public event Action<float> OnStaminaChanged; //값이 바뀔 때마다 "현재 스테미너 원본값"을 방송
        public event Action OnMaxStaminaChanged; // 그릇 개수(최대치)가 바뀌었을 때 알림

        private int lastValidatedCount = -1; // 인스펙터 변화 감지용 (테스트용)

        public void OnValidate() // 인스펙터에서 값이 바뀔 때 호출됨 (BaseCharacter.OnValidate에서 수동으로 불러줘야 함)
        {
            if (staminaCount != lastValidatedCount)
            {
                lastValidatedCount = staminaCount;
                OnMaxStaminaChanged?.Invoke();
            }
        }

        public void AddCount(int amount = 1) // 실제 게임 로직(보상 등)에서 그릇을 늘릴 때 호출하는 함수
        {
            staminaCount += amount;
            curStamina += amount * staminaPerSegment; // 늘어난 만큼 바로 채워줌
            lastValidatedCount = staminaCount; // OnValidate가 중복으로 또 반응하지 않도록 동기화
            OnMaxStaminaChanged?.Invoke(); // 그릇 개수 바뀌었다고 UI에 알림
            OnStaminaChanged?.Invoke(curStamina); // 채워진 값도 같이 알림
        }

        protected override void OnInitialized() // Feature가 Host에 붙을 때 1회 호출되는 초기화 지점
        {
            base.OnInitialized(); // 부모 클래스의 초기화 로직도 실행
            Host.TryGetFeature(out moveFeature); // 같은 Host(캐릭터)에 붙어있는 CharacterMoveFeature를 찾아 캐싱

            curStamina = MaxStamina; // 시작할 때는 스테미너를 가득 채워둠
            lastValidatedCount = staminaCount; // 시작 시점 값으로 초기화 (불필요한 첫 알림 방지)
            OnStaminaChanged?.Invoke(curStamina); // UI가 처음 값도 바로 반영할 수 있도록 1회 방송
        }

        public void Tick(float deltaTime) // 매 틱마다 호출되어 스테미너를 갱신
        {
            bool isSprinting = moveFeature.isMove && moveFeature.IsSprint; // 지금 실제로 움직이면서 스프린트 중인지 판단

            float before = curStamina; // 변화 감지를 위해 갱신 전 값을 저장

            if (isSprinting) 
            {
                curStamina -= drainPerSecond * deltaTime; // 시간에 비례해서 소모
            }
            else 
            {
                curStamina += regenPerSecond * deltaTime; // 시간에 비례해서 회복
            }

            curStamina = Mathf.Clamp(curStamina, 0f, MaxStamina); // 0~최대치 범위를 벗어나지 않게 고정

            if (curStamina <= 0f && moveFeature.IsSprint) // 스테미너가 바닥났는데 아직 스프린트 중이면
            {
                moveFeature.SetSprint(false); // 강제로 스프린트 해제 
            }

            if (curStamina != before)
            {
                Debug.Log($"스테미너: {curStamina:F1} / {MaxStamina}");
                OnStaminaChanged?.Invoke(curStamina); // 구독 중인 UI에 새 원본값을 방송 (원본값 전달로 변경)
            }
        }
    }
}

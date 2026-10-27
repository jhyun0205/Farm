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
        [SerializeField] private float maxStamina = 180f; //스테미너 최대치
        [SerializeField] private float drainPerSecond = 10f; // 초당 소모량 (스프린트 중)
        [SerializeField] private float regenPerSecond = 15f; // 초당 회복량 (스프린트 멈췄을 때)
        [SerializeField] private float staminaPerSegment = 60f; //stamina 1칸이 담당하는 스테미너 양

        private float curStamina; // 현재 스테미너 값
        private CharacterMoveFeature moveFeature; // 이동/스프린트 상태를 확인하기 위한 참조

        public float StaminaRatio => curStamina / maxStamina; // UI에 넘겨줄 0~1 비율 값
        public float CurStamina => curStamina; //현재 스테미너 원본값 (조각별 계산에 필요)
        public float MaxStamina => maxStamina; //최대 스테미너 원본값 (나중에 늘어날 수 있는 값이라 직접 노출)
        public float StaminaPerSegment => staminaPerSegment; //  조각 1칸당 용량
        public int SegmentCount => Mathf.CeilToInt(maxStamina / staminaPerSegment); // 지금 maxStamina 기준으로 몇 조각이 필요한지 계산

        public event Action<float> OnStaminaChanged; //값이 바뀔 때마다 "현재 스테미너 원본값"을 방송

        protected override void OnInitialized() // Feature가 Host에 붙을 때 1회 호출되는 초기화 지점
        {
            base.OnInitialized(); // 부모 클래스의 초기화 로직도 실행
            Host.TryGetFeature(out moveFeature); // 같은 Host(캐릭터)에 붙어있는 CharacterMoveFeature를 찾아 캐싱

            curStamina = maxStamina; // 시작할 때는 스테미너를 가득 채워둠
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

            curStamina = Mathf.Clamp(curStamina, 0f, maxStamina); // 0~최대치 범위를 벗어나지 않게 고정

            if (curStamina <= 0f && moveFeature.IsSprint) // 스테미너가 바닥났는데 아직 스프린트 중이면
            {
                moveFeature.SetSprint(false); // 강제로 스프린트 해제
            }

            if (curStamina != before)
            {
                Debug.Log($"스테미너: {curStamina:F1} / {maxStamina}");
                OnStaminaChanged?.Invoke(curStamina); // 구독 중인 UI에 새 원본값을 방송 (원본값 전달로 변경)
            }
        }
    }
}

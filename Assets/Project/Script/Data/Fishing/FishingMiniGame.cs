using CoreEngine;
using CoreEngine.EventBus;
using Farm.Input;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Farm.Fishing
{
    public class FishingMiniGame : CoreMonoBehaviour, ITickable
    {
        [Header("UI 참조")]
        [SerializeField] private RectTransform bar;
        [SerializeField] private RectTransform targetBox;
        [SerializeField] private RectTransform arrow;
        [SerializeField] private Image gaugeFillImage;

        [Header("목표존 설정")]
        [SerializeField] private float targetBoxWidthRatio = 0.2f; 
        [SerializeField] private float targetBoxSpeed = 0.2f;
        [SerializeField] private float miniGameDuration = 10f; 

        
        private const float ArrowSpeed = 1.5f;

        [Header("게이지 설정 (인스펙터 조절)")]
        [SerializeField] private int successGaugeGain = 10;
        [SerializeField] private int failGaugeLoss = 10;
        // bar의 절반 너비
        private float barHalfWidth;
        //targetBox 좌우 범위
        private float targetBoxRange;
        //targetBox 랜덤 스폰 위치 
        private float targetBoxTime;
        // Arrow 좌우 범위
        private float arrowRange;
        // Gauge 값
        private int gaugeValue;

        // 미니게임 흘러간 시간
        private float elapsedTime; 
        // 미니게임 판정 처리
        private bool isFinished;  

        // 이벤트 알림 
        public event Action OnMiniGameSuccess;
        public event Action OnMiniGameFail;

        // Tick 그룹을 정해줘 실행 위치 지정
        public TickGroup TickGroup => TickGroup.Ui;

        // 실행시 처음 실행되는 함수 
        private void Awake()
        {
            // bar의 너비를 절반을 구함
            barHalfWidth = bar.rect.width / 2f;
            // targetBox size를 세팅
            UpdateTargetBoxSize(targetBoxWidthRatio);
            // 화살표가 bar의 너비만큼 움직일 수 있게
            arrowRange = barHalfWidth - (arrow.sizeDelta.x / 2f);
            //targetBox의 위치가 랜덤으로 스폰
            targetBoxTime = UnityEngine.Random.Range(0f, 4f);
        }
        // 좌클릭 입력을 구독
        protected override void OnEnable()
        {
            base.OnEnable();
            EventBus<UseItemEvent>.Subscribe(OnClickInput);
        }
        // 구독 해제
        protected override void OnDisable()
        {
            base.OnDisable();
            EventBus<UseItemEvent>.Unsubscribe(OnClickInput);
        }

        public void Tick(float deltaTime)
        {
            // 미니게임이 끝난 상태 확인
            if (isFinished) return; 
            // 현재 흐르는 시간을 누적
            elapsedTime += deltaTime; 
            // 누적된 경과 시간이 제한시간을 초과하면
            if (elapsedTime >= miniGameDuration) 
            {
                //실패처리
                FinishMiniGame(isSuccess: false);
                return;
            }
            // 경과 시간에 맞게 targetBox 크기 갱신
            UpdateTargetBoxWidthByTime();
            // targetBox 좌우로 이동
            MoveTargetBox();
            // Arrow 좌우로 이동
            MoveArrow();
        }

        //경과 시간에 비율에 맞춰 targetbox 크기를 이징함수를 이용해 키움
        private void UpdateTargetBoxWidthByTime()
        {
            //흐른 시간을 게임시간으로 나눠 정규화
            float timeRatio = Mathf.Clamp01(elapsedTime / miniGameDuration);
            // Ease-Out Quad 공식 : 1 - (1-t)^2
            // Ease-In Quad 공식 : t * t (처음엔 느리게 나중에 빠르게
            // Ease-In-Out Quad 공식 : float growFactor = timeRatio < 0.5f
            // ? 2f * t * t
            // : 1f - Mathf.Pow(-2f * t + 2f, 2) / 2f;

            //처음에 빠르게 커지고 종료시간에 가까워질수록 속도가 천천히 커짐
            float growFactor = 1f - (1f - timeRatio) * (1f - timeRatio); 
            // targetBox 크기가 실제
            UpdateTargetBoxSize(targetBoxWidthRatio * growFactor);
        }

        private void UpdateTargetBoxSize(float widthRatio)
        {
            Vector2 size = targetBox.sizeDelta;
            size.x = bar.rect.width * widthRatio;
            targetBox.sizeDelta = size;

            targetBoxRange = barHalfWidth - (size.x / 2f);
        }

        private void MoveTargetBox()
        {
            float t = Mathf.PingPong(Time.time * targetBoxSpeed + targetBoxTime, 2f) - 1f;
            targetBox.anchoredPosition = new Vector2(t * targetBoxRange, targetBox.anchoredPosition.y);
        }

        private void MoveArrow()
        {
            float t = Mathf.PingPong(Time.time * ArrowSpeed, 2f) - 1;
            arrow.anchoredPosition = new Vector2(t * arrowRange, arrow.anchoredPosition.y);
        }

        private void OnClickInput(UseItemEvent evt)
        {
            if (isFinished) return; 
            JudgeClick();
        }

        private void JudgeClick()
        {
            float boxHalfWidth = targetBox.sizeDelta.x / 2f;
            float boxMin = targetBox.anchoredPosition.x - boxHalfWidth;
            float boxMax = targetBox.anchoredPosition.x + boxHalfWidth;

            bool isHit = arrow.anchoredPosition.x >= boxMin && arrow.anchoredPosition.x <= boxMax;

            gaugeValue += isHit ? successGaugeGain : -failGaugeLoss;
            gaugeValue = Mathf.Clamp(gaugeValue, 0, 100);

            gaugeFillImage.fillAmount = gaugeValue / 100f;

            Debug.Log(isHit ? $"성공! 게이지: {gaugeValue}" : $"실패! 게이지: {gaugeValue}");

            if (gaugeValue >= 100)
            {
                FinishMiniGame(isSuccess: true);
            }
        }

        private void FinishMiniGame(bool isSuccess)
        {
            isFinished = true;

            if (isSuccess)
            {
                Debug.Log("미니게임 성공!");
                OnMiniGameSuccess?.Invoke();
            }
            else
            {
                Debug.Log("미니게임 실패!");
                OnMiniGameFail?.Invoke();
            }

            Destroy(gameObject);
        }
    }
}
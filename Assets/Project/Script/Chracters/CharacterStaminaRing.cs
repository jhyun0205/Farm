using CoreEngine;
using CoreEngine.Actor;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Farm.Character
{
    public class CharacterStaminaRing : BaseActorFeature, IDisposable , ITick
    {
        private GameObject canvasRoot; // 전체를 켜고 끌 대상 Canvas 오브젝트 (이름 변경: ringRoot -> canvasRoot)
        private readonly List<GameObject> ringRoots = new();
        private readonly List<Image> ringFills = new(); //  각 그릇의 "Fill" Image를 미리 찾아 캐싱해둘 목록
        private CharacterStaminaFeature staminaFeature; //  구독 해제를 위해 보관
        private float baseOffsetX;
        private float baseScaleX;

        protected override void OnInitialized() // CoreMonoBehaviour의 OnEnable 대신 Feature의 초기화 지점
        {
            base.OnInitialized();

            canvasRoot = Host.transform.Find("StaminaRing").gameObject;
            baseOffsetX = Mathf.Abs(canvasRoot.transform.localPosition.x);
            baseScaleX = Mathf.Abs(canvasRoot.transform.localScale.x);
            canvasRoot.SetActive(false); // 구독 전까지 숨김

            CacheImages(); // ringRoots 안의 "Fill" Image들을 미리 찾아둠 (Instantiate가 없으니 한 번만 캐싱하면 됨)

            if (Host.TryGetFeature(out staminaFeature))
            {
                UpdateVisibleCount(staminaFeature.SegmentCount); // 지금 보유한 그릇 개수만큼만 켜기
                UpdateFill(staminaFeature.CurStamina);
                staminaFeature.OnStaminaChanged += UpdateFill;
                staminaFeature.OnMaxStaminaChanged += OnVesselCountChanged; // 그릇 개수가 바뀔 때마다 다시 그리기
            }
        }

        public void Dispose()
        {
            if (staminaFeature != null)
            {
                staminaFeature.OnStaminaChanged -= UpdateFill;
                staminaFeature.OnMaxStaminaChanged -= OnVesselCountChanged;
            }
        }

        private void OnVesselCountChanged() // 그릇 개수가 바뀔 때만 호출됨 (테스트: 인스펙터 수정 / 실제: AddVessel 호출)
        {
            UpdateVisibleCount(staminaFeature.SegmentCount);
        }

        private void CacheImages() //  미리 배치된 각 그릇에서 맞는 자식의 Image를 찾아 저장
        {
            ringRoots.Clear();
            ringFills.Clear();
            
            foreach (Transform child in canvasRoot.transform)
            {
                ringRoots.Add(child.gameObject);
                ringFills.Add(child.GetComponent<Image>());
            }

            ringRoots.Reverse();
            ringFills.Reverse();
        }

        private void UpdateVisibleCount(int count) //  인덱스가 count보다 작은 그릇만 켜고 나머지는 끔
        {
            for (int i = 0; i < ringRoots.Count; i++)
            {
                ringRoots[i].SetActive(i < count);
            }
        }

        private void UpdateFill(float curStamina) //  현재 값으로 보이는 그릇들의 채움 비율 갱신
        {
            bool shouldShow = curStamina < staminaFeature.MaxStamina;
            canvasRoot.SetActive(shouldShow);

            for (int i = 0; i < ringFills.Count; i++)
            {
                if (!ringRoots[i].activeSelf) continue; // 꺼져있는(아직 없는) 그릇은 계산 안 함

                float segmentStart = i * staminaFeature.StaminaPerSegment;
                float remaining = curStamina - segmentStart;
                ringFills[i].fillAmount = Mathf.Clamp01(remaining / staminaFeature.StaminaPerSegment);
            }
        }

        public void Tick(float deltaTime)
        {
            if (canvasRoot == null) return;

            float facing = Host.transform.localScale.x;

            Vector3 scale = canvasRoot.transform.localScale;
            scale.x = baseScaleX * facing; // 원래 크기를 유지하면서 부호만 바꿈
            canvasRoot.transform.localScale = scale;

            Vector3 pos = canvasRoot.transform.localPosition;
            pos.x = baseOffsetX * facing;
            canvasRoot.transform.localPosition = pos;
        }
    }
}
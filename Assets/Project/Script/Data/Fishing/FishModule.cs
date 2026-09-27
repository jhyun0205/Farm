using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Farm.Fishing
{
    public class FishModule : BaseCharacterModule, IActorFeature
    {
        private const int FISHING_ROD_INDEX = 4; // 낚싯대 아이템 index

        //낚시대를 들고있는지 확인하기위해
        private CharacterInventory inventory;
        // 릴 전환 입력 구독을 위한 참조
        private UserInputManager inputManager;
        //지금 낚시대를 장착중인가
        private bool isFishingRodEquipped;
        // 낚시 최소, 최대 대기시간
        private const float Min_Wait = 5f;
        private const float Max_Wait = 20f;
        // ? 뜨고 유지시간
        private const float Question_Duration = 2f;
        // ! 뜨고 유지시간
        private const float Bite_Duration = 2f;
        // ? 이후 !로 넘어갈 확률 
        private const float Bite_Chance = 0.7f;

        //캐스팅~판정까지 전체 시퀀스 진행 여부
        private bool isFishingActive;
        // ! 구간 성공 가능인지
        private bool isBiteCanFish;
        // 실행중긴 대기/ 판정 코루틴을 나중에 강제 중단하기 위해 보관
        private Coroutine fishingCoroutine;
        // 실행중인 이동 잠금 코루틴을 취소하기 위해 보관
        private Coroutine unlockCoroutine;

        // 애니메이션 재생시간 기준 값
        private const float End_ANIM_DURATION = 0.5f;

        //낚싯대 릴 개수 (기본, 하, 중, 상)
        public int reelTier { get; private set; } = 0;
        // 상태 뷰를 알리기 위한 방송
        public event Action<EmojiType> Event_OnBiteIconChanged;

        //캐릭터가 모듈에 뭍었을때 한번 실행 컴포넌트 참조 연결, 이벤트 구독
        public override void Initialize(BaseCharacter owner)
        {
            base.Initialize(owner);
            _anim = owner.GetComponent<baseCharacterAnim>();

            inventory = owner.GetFeature<CharacterInventory>();
            inventory.Event_OnSelectedSlotChanged += OnSelectedSlotChanged;

            inputManager = GameManager.GetManager<UserInputManager>();
            inputManager.Event_OnSwitchReelInput += CycleReelTier;

            //타일맵 캐싱
            upperLayers = terrainGrid.GetComponentsInChildren<Tilemap>();
        }

        //캐릭터가 사라질때 실행 구독 전부 해제
        public override void Exit()
        {
            inventory.Event_OnSelectedSlotChanged -= OnSelectedSlotChanged;
            inputManager.Event_OnSwitchReelInput -= CycleReelTier;
        }

        //===낚싯대 장비 여부===

        //지금 든게 낚시대인지 아닌지 판단하기 위한 함수
        private void OnSelectedSlotChanged(int index)
        {
            ItemDataContainer item = inventory.GetItem(index);
            isFishingRodEquipped = !item.IsEmpty() && item.Get().Index == FISHING_ROD_INDEX;

            // 낚시 진행중 다른 장비로 교체 시
            if (isFishingActive && !isFishingRodEquipped)
            {
                // 중단처리
                ResolveMiss(isGenuineMiss: false);
            }
        }

        //낚시대를 들고있을때만 릴 단계를 변경할수있게
        public void CycleReelTier()
        {
            if (!isFishingRodEquipped) return;

            reelTier = (reelTier + 1) % 4;
            Debug.Log($"릴 단계: {reelTier}");
        }

        // === 물고기 획득 로직 ===

        //현재 reelTier 기준으로 기본에서 해당단계 테이블을 합쳐 가중치로 비례하여 물고기 랜덤획득
        public FishData RollFish()
        {
            //GetPool로 릴 단계가 허용하는 Pool을 받아옴
            List<FishData> pool = table.GetPool(reelTier);
            
            float totalWeight = 0f;
            foreach (FishData fish in pool)
            {
                totalWeight += fish.weight;
            }

            float roll = UnityEngine.Random.Range(0f, totalWeight);
            float cumulative = 0f;
            foreach (FishData fish in pool)
            {
                cumulative += fish.weight;
                if (roll <= cumulative)
                {
                    return fish;
                }
            }
            return pool[pool.Count - 1];
        }

        //=== 방향추적 ===

        [SerializeField] private Tilemap waterTilemap; //인스펙터에서 씬의 "Water"Tilemap 오브젝트 가져오기
        [SerializeField] private Grid terrainGrid; //인스펙터에서 위쪽 레이어부터 순서대로 넣기
        [SerializeField] private FishTable table;

        private Tilemap[] upperLayers;

        private Vector2 lastFacingDir = Vector2.down;
        baseCharacterAnim _anim;

        //결과를 밖으로 방송
        public event Action<bool> Event_OnFishingResult;

        //플레이어가 바라보는 방향 갱신, 이동 감지해서 강제 중단
        public override void Tick(float deltaTime)
        {
            if (Owner.isMove)
            {
                lastFacingDir = SnapToCardinal(Owner.inputMove);
            }

            if (isFishingActive && Owner.inputMove.sqrMagnitude > 0.01f)
            {
                ResolveMiss(isGenuineMiss: false);
            }
        }

        // 좌클릭시 호출되고 캐스팅 시작 or 판정처리
        public void TryFish()
        {
            if (isFishingActive)
            {
                if (isBiteCanFish)
                {
                    ResolveCatch();
                }
                else
                {
                    ResolveMiss();
                }
                return;
            }

            //판정 애니메이션 잠금 중이면 새 캐스팅 시도 자체를 무시
            if (!Owner.canMove) return; 

            bool isFishing = IsSeaTile();
            //결과를 이벤트로 방송
            Event_OnFishingResult?.Invoke(isFishing);

            if (isFishing)
            {
                StartFishing();
            }
            else
            {
                Debug.Log("낚시할 곳이 없습니다.");
            }

        }
        // 낚시 캐스팅 시작
        private void StartFishing()
        {
            Debug.Log("낚시중");
            _anim.SetIsFishing(true);
            isFishingActive = true;
            Owner.canMove = false;
            if (unlockCoroutine != null) StopCoroutine(unlockCoroutine);
            fishingCoroutine = StartCoroutine(FishingSequence());
        }

        //정면 셀이 맨 위에가 물인가 
        public bool IsSeaTile()
        {
            Vector3Int frontCell = GetFrontCell();

            if (!waterTilemap.HasTile(frontCell))
            {
                return false;
            }

            foreach (Tilemap layer in upperLayers)
            {
                if (layer == waterTilemap)
                {
                    continue;
                }
                if ((layer.HasTile(frontCell)))
                {
                    return false;
                }
            }
            return true;
        }

        //캐릭터 1칸 위치값 실수에서 정수로 변환후 반환 WorldToCell 이용
        private Vector3Int GetFrontCell()
        {
            // 캐릭터 위치에서 바라보는 방향으로 1칸 만큼 이동한 월드좌표를 구함
            Vector3 frontWorldPos = Owner.transform.position + (Vector3)lastFacingDir;
            //2d라서 z 값 고정
            frontWorldPos.z = 0f;
            // 정수로 변환후 반환
            return waterTilemap.WorldToCell(frontWorldPos);
        }

        //플레이어가 움직일때 정면이 어느방향인지 체크
        private Vector2 SnapToCardinal(Vector2 dir)
        {
            //상하좌우 정리
            return Mathf.Abs(dir.x) > Mathf.Abs(dir.y)
                ? new Vector2(Mathf.Sign(dir.x), 0)
                : new Vector2(0, Mathf.Sign(dir.y));
        }
        //물고기 ? ~ ! 가 되는 과정
        private IEnumerator FishingSequence()
        {
            yield return new WaitForSeconds(UnityEngine.Random.Range(Min_Wait, Max_Wait));

            // "?" 상태 — 70% 뽑힐 때까지 반복
            while (true)
            {
                Event_OnBiteIconChanged?.Invoke(EmojiType.Question);
                float questionTimer = 0f;
                while (questionTimer < Question_Duration)
                {
                    if (!isFishingActive) yield break;
                    questionTimer += Time.deltaTime;
                    yield return null;
                }

                if (UnityEngine.Random.value <= Bite_Chance)
                {
                    break; // 성공 → "!" 단계로 진행
                }
                // 30% 실패 → 다시 "?"부터 반복
            }

            Event_OnBiteIconChanged?.Invoke(EmojiType.Exclamation);
            isBiteCanFish = true;

            float biteTimer = 0f;
            while (biteTimer < Bite_Duration)
            {
                if (!isFishingActive) yield break;
                biteTimer += Time.deltaTime;
                yield return null;
            }

            isBiteCanFish = false;
            ResolveMiss(); // "!" 구간 동안 반응 못함(타임아웃)만 미스
        }

        //성공 판정
        private void ResolveCatch()
        {
            // 상태 초기화 + 코루틴 정지
            EndFishingSequence();
            // 움직이지 못하게 false
            Owner.canMove = false;
            // 성공 애니메이션 재생
            _anim.SetFishCatch();
            Debug.Log("물고기를 잡았다!");
            //애니메이션 길이만큼 정지시키고 이동잠금 해제 애니메이션 끝난 뒤 자동 재캐스팅
            LockMovementDuring(End_ANIM_DURATION, autoRecast: true);
        }
        //실패 판정
        private void ResolveMiss(bool isGenuineMiss = true)
        {
            //상태 초기화와 코루틴 정지
            EndFishingSequence();
            // 판정 애니메이션 동안 움직이지 못하게 false
            Owner.canMove = false;
            // 실패 판정 애니메이션 재생
            _anim.SetFishMiss();
            // 애니메이션이 나왔다면 실패 or 정지 인지 확인후 출력
            if (isGenuineMiss)
            {
                Debug.Log("물고기를 놓쳤다...");
               // Event_OnBiteIconChanged?.Invoke(EmojiType.Exclamation);나중에 슬픈 이모지로 변경하셈  
            }
            else
            {
                Debug.Log("낚시 중단");
            }
            //애니메이션 중단 
            LockMovementDuring(End_ANIM_DURATION);
        }
        // 낚시 상태 초기화하고 진행중인 코루틴 정지
        private void EndFishingSequence()
        {
            //캐스팅 판정을 꺼서 새로운 캐스팅을 하게
            isFishingActive = false;
            //! 구간 상태도 같이 꺼서 다음 낚시에 잘못된 값이 남지 않게 
            isBiteCanFish = false;
            // 이모지를 숨김 방송
            Event_OnBiteIconChanged?.Invoke(EmojiType.None);
            //대기 판정이 null이 아니라면
            if (fishingCoroutine != null)
            {   //강제로 멈춰서 중복 방지
                StopCoroutine(fishingCoroutine);
            }
        }
        //지정된 시간 만큼 기다렸다가 이동 장금 해제
        private IEnumerator UnlockMoveAfterDelay(float delay, bool autoRecast)
        {
            // 딜레이 초만큼 멈췄다가 다시 이어감 
            yield return new WaitForSeconds(delay);
            //시간 되면 이동 잠금 해제
            Owner.canMove = true;
            
            if (autoRecast && IsSeaTile())// 캐치로 인한 잠금해제 +  여전히 바다를 보고있다면 
            {
                StartFishing(); // 캐스팅 시작
            }
        }
        //이전 장금해제 예약 취소하고 새로 예약을 걸기 호출
        private void LockMovementDuring(float duration, bool autoRecast = false)
        {
            // unlock 값이 null이 아니면
            if (unlockCoroutine != null) 
            {
                // 초기화
                StopCoroutine(unlockCoroutine);
            }
            //duration 뒤에 잠금 풀리게 예약하고 코루틴 참조를 저장
            unlockCoroutine = StartCoroutine(UnlockMoveAfterDelay(duration, autoRecast));
        }
    }
}
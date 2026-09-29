using CoreEngine;
using CoreEngine.Actor;
using CoreEngine.Helpers;
using UnityEngine;

namespace Farm.Character.Move
{
    [System.Serializable]
    public class CharacterMoveFeature : BaseActorFeature, IFixedTick
    {
        public Vector2 inputMove;
        public bool isMove;
        public float moveSpeed = 6f;
        public float SprintMul = 2f;

        public bool IsSprint { get; private set; }
        private Rigidbody2D _rigidbody;

        // 낚시 중 다른 액션 이동을 잠그는 용
        public bool canMove = true;

        protected override void OnInitialized()
        {
            base.OnInitialized();
            if(!Host.TryGetComponent(out _rigidbody))
            {
                LogHelper.LogWarning($"[{Host?.name}]에 {nameof(Rigidbody2D)} 없음");
            }
        }

        public void FixedTick(float fixedDeltaTime)
        {
            Physics_Move(fixedDeltaTime);
        }

        public virtual void Move(Vector2 input)
        {
            inputMove = input;
            isMove = input.sqrMagnitude > 0.01f;

            // Scale -1을 이용한 좌우 반전 로직
            // x값이 0일 때는 마지막 방향을 유지하기 위해 '0이 아닐 때만' 업데이트
            if (input.x != 0)
            {
                float direction = input.x > 0 ? 1f : -1f;

                // 부모의 Scale을 뒤집어 하위 무기, 이펙트 위치까지 한꺼번에 반전
                Host.transform.localScale = new Vector3(direction, 1f, 1f);
            }
            // 위 아래 일 시 정상 scale로 변경
            else if (input.y != 0)
            {
                Host.transform.localScale = Vector3.one;
            }
        }

        private void Physics_Move(float fixedDataTime)
        {
            if(isMove && canMove)
            {
                // 탑다운 선형 움직임
                Vector2 nextVec = inputMove.normalized * moveSpeed;
                if (IsSprint) nextVec *= SprintMul;
                _rigidbody.linearVelocity = nextVec; 

                // 가속 감속 움직임
                //Vector2 nextVec = inputMove.normalized * moveSpeed * fixedDataTime;
                //if (IsSprint) nextVec *= SprintMul;
                //_rigidbody.AddForce(nextVec, ForceMode2D.Impulse);
            }
            else
            {
                _rigidbody.linearVelocity = Vector2.zero;
            }
        }

        public void SetSprint(bool isSprint)
        {
            IsSprint = isSprint;
        }
    }
}

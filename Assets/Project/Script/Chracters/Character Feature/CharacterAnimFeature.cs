using CoreEngine.Actor;
using UnityEngine;
using UnityEngine.Windows;
using CoreEngine.Animation;

namespace Farm.Character
{
    [System.Serializable]
    public class CharacterAnimFeature : BaseAnimFeature
    {
        int Hash_InputX;
        int Hash_InputY;
        int Hash_IsMove;
        int Hash_IsSprint;
        int Hash_IsFishing;
        int Hash_FishCatch;
        int Hash_FishMiss;

        protected override void OnInitialized()
        {
            base.OnInitialized();
            GetAnimPrarmHash();
        }


        protected override void GetAnimPrarmHash()
        {
            Hash_InputX = Animator.StringToHash("InputX");
            Hash_InputY = Animator.StringToHash("InputY");
            Hash_IsMove = Animator.StringToHash("IsMove");
            Hash_IsSprint = Animator.StringToHash("IsSprint");
            Hash_IsFishing = Animator.StringToHash("isFishing"); 
            Hash_FishCatch = Animator.StringToHash("FishCatch");
            Hash_FishMiss = Animator.StringToHash("FishMiss");
        }

        public void SetInputMove(Vector2 inputMove)
        {
            SetParam(Hash_InputX, inputMove.x);
            SetParam(Hash_InputY, inputMove.y);
        }

        public void SetIsMove(bool isMove) => SetParam(Hash_IsMove, isMove);
        public void SetIsSprint(bool isRun) => SetParam(Hash_IsSprint, isRun);

        public void SetIsFishing(bool isFishing) => SetParam(Hash_IsFishing, isFishing); // bool값
        public void SetFishCatch() => SetParam(Hash_FishCatch);
        public void SetFishMiss() => SetParam(Hash_FishMiss);

    }
}


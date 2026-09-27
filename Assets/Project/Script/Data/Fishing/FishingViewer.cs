using UnityEngine;

namespace Farm.Fishing
{
    public class FishingViewer : MonoBehaviour
    {
        //이벤트를 구독할 대상
        private FishModule fishModule;
        // 이모지를 화면에 그려줄 대상
        private CharacterEmoji characterEmoji;

        private void Awake()
        {
            fishModule = GetComponentInParent<FishModule>(); // FishModule을 찾아서 참조 저장
            characterEmoji = GetComponent<CharacterEmoji>(); // 오브젝트에 CharacterEmoji참조 저장
        }

        private void OnEnable()
        {
            // 오브젝트가 활성화될때 FishModule의 방송을 구독
            fishModule.Event_OnBiteIconChanged += OnBiteIconChanged;
        }

        private void OnDisable()
        {
            // 비활성화 될때 구독 해제
            fishModule.Event_OnBiteIconChanged -= OnBiteIconChanged;
        }

        private void LateUpdate()
        {
            //부모 좌우반전 이모지는 정방향으로 보이게
            Vector3 parentScale = transform.parent.localScale;
            transform.localScale = new Vector3(parentScale.x, 1f, 1f);
        }
        
        private void OnBiteIconChanged(EmojiType type)
        {
            if (type == EmojiType.None)
            {
                characterEmoji.Hide();//None이면 숨김
            }
            else
            {
                characterEmoji.Show(type); // None이 아니면 해당 이모지 표시
            }
        }
    }
}
using UnityEngine;

// 이모지 종류 목록
public enum EmojiType { None, Question, Exclamation }

// 이거 붙이면 spriteRenderer 도 없으면 자동으로 추가됨
[RequireComponent(typeof(SpriteRenderer))]
public class CharacterEmoji : MonoBehaviour
{
    //인스펙터 창에 직렬화 가능하게 표시
    [System.Serializable]
    private struct EmojiEntery
    {
        public EmojiType type; //이모지 종류
        public Sprite sprite; // 스프라이트 이미지
    }

    private SpriteRenderer emojiRenderer; //렌더러 참조
    [SerializeField] private EmojiEntery[] emojis;  //인스펙터에서 등록한 타입-스프라이트 매칭 목록

    private void Awake()
    {
        //같은 오브젝트에 붙어있는 스프라이트렌더러 찾아서 참조 저장
        emojiRenderer = GetComponent<SpriteRenderer>();
    }

    public void Show(EmojiType type)
    {
        foreach (var entry in emojis) // 등록한 이모지 목록 순회
        {
            if (entry.type == type)
            {
                // 타입에 맞는 스프라이트로 교체
                emojiRenderer.sprite = entry.sprite;
                //렌더러를 켜 화면에 보이게 함
                emojiRenderer.enabled = true;
                return;
            }
        }
        Debug.LogWarning($"{type}에 대응하는 스프라이트가 등록 안 됨");
    }

    public void Hide()
    {
        emojiRenderer.enabled = false; // 이모지 화면에서 가리기
    }
}
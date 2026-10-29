using UnityEngine;

namespace CoreEngine.TileSystem.Contracts
{
    /// <summary>
    /// 마우스 포인터 및 록온 타일 하이라이트 표시 인터페이스
    /// </summary>
    public interface ITileCheckerView
    {
        // 셀 좌표나 상태가 변경되었을 때만 SubCanvas 내부에서 커서 갱신
        void UpdateChecker(Vector2Int cellPos, Vector3 worldCenter, TileCheckerState state);

        // GameObject.SetActive 대신 CanvasGroup.alpha를 조절하여 숨김/표시 전환
        void SetVisible(bool isVisible);
    }
}
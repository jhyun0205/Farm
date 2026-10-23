using UnityEngine;
using UnityEngine.Tilemaps; // 타일맵 관련 기능을 위해 필수!
using UnityEngine.InputSystem;
using CoreEngine.Actor;
using CoreEngine;
using CoreEngine.Helpers;

/// <summary>
/// 캐릭터가 현재 타겟으로하는 타일 확인해주는 객체
/// </summary>
[System.Serializable]
public class CharacterTileChecker : BaseActorFeature, IActorFeature, ITick
{
    [SerializeField] private Tilemap targetTilemap;   // 현재 사용 중인 타일맵
    [SerializeField] private GameObject tileMarker;   // 타일을 표시할 view
    private Renderer[] highlightRenderers;
    private BoxCollider2D tileMarkerCollider;
    private Vector3 tileMarkerPos; // 타일마커의 현재 위치

    protected override void OnInitialized()
    {
        base.OnInitialized();
        highlightRenderers = tileMarker.GetComponentsInChildren<Renderer>();
        tileMarkerCollider = tileMarker.GetComponent<BoxCollider2D>();
    }

    public void Tick(float deltaTime)
    {
        TileCheck();
    }

    private void TileCheck()
    {
        Vector3 curPos = Host.transform.position;
        // TODO 캐릭터 정면 +0.5 만큼 위치 이동
        curPos.z = 0f;

        if(targetTilemap == null)
        {
            //LogHelper.LogWarning("targetTilemap 동적 관리 필요");
            return;
        }

        // 월드 좌표를 타일맵의 '셀(칸) 좌표'로 변환
        Vector3Int cellPosition = targetTilemap.WorldToCell(curPos);

        // 해당 셀에 타일이 있는지 확인 (선택 사항: 타일이 있는 곳만 하이라이트 하고 싶을 때)
        if (targetTilemap.HasTile(cellPosition))
        {
            foreach (Renderer rd in highlightRenderers)
            {
                rd.enabled = true;
            }

            // 셀 좌표의 중앙 월드 좌표를 가져와서 하이라이트 오브젝트 위치 수정
            tileMarkerPos = targetTilemap.GetCellCenterWorld(cellPosition);
            tileMarker.transform.position = tileMarkerPos;
        }
        else
        {
            // 타일이 없는 허공이라면 하이라이트 끄기
            foreach (Renderer rd in highlightRenderers)
            {
                rd.enabled = false;
            }
        }
    }

    // 실제 객체의 위치는 플레이어에 의해 바뀔 수 있으니 저장된 속성을 사용
    public Vector3 GetTilePosition() => tileMarkerPos;

    public BoxCollider2D GetTileMarkerBoxCollider() => tileMarkerCollider;

}
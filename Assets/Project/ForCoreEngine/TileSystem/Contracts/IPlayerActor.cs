using CoreEngine.EventBus;
using System;
using UnityEngine;

namespace CoreEngine.TileSystem.Contracts
{
    // 유저가 수동 이동 입력(WASD 등)을 했을 때 자동 이동이나 록온을 취소하기 위한 이벤트
    public struct ManualMoveInputDetectedEvent : IEvent { }

    /// <summary>
    /// 타일 시스템이 플레이어 컨트롤러 클래스를 직접 참조하지 않고 위치 조회 및 이동을 요청하는 규격 (기능 3.1, 5)
    /// </summary>
    public interface IPlayerActor
    {

        Vector2Int CurrentCell { get; }
        Vector2Int FacingDirection { get; } // 시선 방향 단위 벡터: (0,1), (1,0), (0,-1), (-1,0)
        bool IsMoving { get; }

        // 대상 타일 작업이 가능한 인접 셀까지 캐릭터 이동 요청
        void MoveToAdjacentCell(Vector2Int targetCell, Action onArrived, Action onFailed);

        // 진행 중인 자동 이동 명령 즉시 중단
        void CancelAutoMovement();
    }
}
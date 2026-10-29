namespace CoreEngine.TileSystem.Contracts
{
    /// <summary>
    /// 규칙 기반 오토타일 비트마스크 연산 모드
    /// </summary>
    public enum BitmaskRuleType : byte
    {
        None = 0,       // 단일 고정 스프라이트 (규칙 없음)
        Cardinal4 = 1,  // 상하좌우 4방향 (0 ~ 15, 총 16타일)
        Blob8 = 2       // 8방향 + 대각선 게이팅 (0 ~ 255 중 47개 고유값)
    }

    /// <summary>
    /// 타일 상호작용 및 타겟팅 FSM 상태
    /// </summary>
    public enum TileInteractionState : byte
    {
        Idle = 0,           // 기본 마우스 추적 상태
        Executing = 1,      // 타일 작업 수행 중 (타일과 직접 상호작용 중)
        LockedOn = 2,       // 작업 중도 중단으로 대상 타일이 고정된 상태 (상호작용 중도 해제시 연결은 해제하지 않은 상태)
        SearchingNext = 3,  // 작업 완료 직후 범위 내 동일 타입 타일 B 탐색 상태 (성공적으로 상호작용이 종료된 상태)
        MovingToTarget = 4  // 시스템에 의한 자동 접근 이동 상태 (선택 타일로 자동이동이 필요한 상태)
    }

    /// <summary>
    /// 선택될 타일 표시View의 명시적 상태
    /// </summary>
    public enum TileCheckerState : byte
    {
        Hidden = 0,         // View를 제공하지 않음
        ValidInRange = 1,   // 사거리 이내 (정상 표시)
        OutOfRange = 2,     // 사거리 밖 (경고 or 반투명 표시 or Clamped)
        LockedOn = 3        // 자동 록온 고정 상태 (선택타일이 커서 위치와 별개)
    }

    /// <summary>
    /// 그리드 8방향 정의 (시계방향 순서, 짝수=직교, 홀수=대각선)
    /// </summary>
    public enum GridDirection : byte
    {
        North = 0,
        NorthEast = 1,
        East = 2,
        SouthEast = 3,
        South = 4,
        SouthWest = 5,
        West = 6,
        NorthWest = 7
    }
}
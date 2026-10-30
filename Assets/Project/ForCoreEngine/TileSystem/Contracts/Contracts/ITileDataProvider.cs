namespace CoreEngine.TileSystem.Contracts
{
    /// <summary>
    /// 그리드 스토리지가 타일 생성 및 규칙 판정을 위해 필요로 하는 메타데이터 규격
    /// </summary>
    public readonly struct TileCreationInfo
    {
        public readonly ulong TileID;
        public readonly byte LayerIndex;
        public readonly int CategoryMask;
        public readonly ulong ConnectGroupID;
        public readonly BitmaskRuleType RuleType;
        public readonly int MaxDurability;

        public TileCreationInfo(
            ulong tileId,
            byte layerIndex,
            int categoryMask,
            ulong connectGroupId,
            BitmaskRuleType ruleType,
            int maxDurability)
        {
            TileID = tileId;
            LayerIndex = layerIndex;
            CategoryMask = categoryMask;
            ConnectGroupID = connectGroupId;
            RuleType = ruleType;
            MaxDurability = maxDurability;
        }
    }

    /// <summary>
    /// 프레임워크가 프로젝트 계층의 마스터 데이터 테이블을 역참조 없이 조회하기 위한 인터페이스
    /// </summary>
    public interface ITileDataProvider
    {
        bool TryGetTileCreationInfo(ulong tileId, out TileCreationInfo info);
    }
}
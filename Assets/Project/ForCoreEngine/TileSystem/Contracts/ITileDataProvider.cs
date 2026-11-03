namespace CoreEngine.TileSystem.Contracts
{
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

    public interface ITileDataProvider
    {
        bool TryGetTileCreationInfo(ulong tileId, out TileCreationInfo info);
    }
}
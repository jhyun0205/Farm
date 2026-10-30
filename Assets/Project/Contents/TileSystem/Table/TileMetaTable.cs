using System;
using CoreEngine.GameData;
using CoreEngine.TileSystem.Contracts;
using UnityEngine;

namespace Farm.GameData
{
    [Serializable]
    public class TileMetaRecord : BaseRecord
    {
        [TableColumn, SerializeField] protected byte layerIndex;
        [TableColumn, SerializeField] protected int categoryMask;
        [TableColumn, SerializeField] protected ulong connectGroupId;
        [TableColumn, SerializeField] protected BitmaskRuleType ruleType;
        [TableColumn, SerializeField] protected int maxDurability;

        public byte LayerIndex => layerIndex;
        public int CategoryMask => categoryMask;
        public ulong ConnectGroupId => connectGroupId;
        public BitmaskRuleType RuleType => ruleType;
        public int MaxDurability => maxDurability;

        public TileCreationInfo ToCreationInfo() =>
            new TileCreationInfo(
                HashedPK,
                layerIndex,
                categoryMask,
                connectGroupId,
                ruleType,
                maxDurability);
    }
    
    public class TileMetaTable : BaseTable<TileMetaRecord>
    {
        public TileMetaRecord GetTypedRecord(ulong id) => GetRecord(id) as TileMetaRecord;
    }
}
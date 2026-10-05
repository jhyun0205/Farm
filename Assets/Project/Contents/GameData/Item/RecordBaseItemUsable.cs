using CoreEngine;
using CoreEngine.GameData;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Farm.GameData.Item
{
    public abstract class RecordBaseItemUsable : RecordBaseItem
    {
        //[TableColumn, SerializeField] AssetId<BaseItemEffect>[] effects_OnUse; // 사용즉시 적용할 효과
        //[TableColumn, SerializeField] AssetId<BaseItemEffect>[] effects_OnAnimEvent; // 사용 후 애니메이션 이벤트로 적용할 효과
        //[TableColumn, SerializeField] AssetId<BaseCondition>[] conditions; // 만족해야 하는 조건
        //[TableColumn, SerializeField] AssetId<BaseCondition>[] constraints; // 만족하면 안되는 조건(제약조건)

        //public AssetId<BaseItemEffect>[] Effects_OnUse => effects_OnUse;
        //public AssetId<BaseItemEffect>[] Effects_OnAnimEvent => effects_OnAnimEvent;
        //public AssetId<BaseCondition>[] Conditions => conditions;
        //public AssetId<BaseCondition>[] Constraints => constraints;


        public virtual bool TryUse(BaseActor user)
        {
            //if (conditions != null)
            //{
            //    // 사용 가능여부 확인
            //    // 배열이 비어있으면 true
            //    bool able = Array.TrueForAll(conditions, condition => condition.Get().IsSatisfied(user));
            //    if (!able) return false;
            //}

            //if (constraints != null)
            //{
            //    // 사용 불가능한지 확인
            //    // 배열이 비어있으면 false
            //    bool disable = Array.Exists(constraints, constraints => constraints.Get().IsSatisfied(user));
            //    if (disable) return false;
            //}
            return true;
        }
    }
}

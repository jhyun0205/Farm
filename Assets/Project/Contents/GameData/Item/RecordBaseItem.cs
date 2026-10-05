using CoreEngine;
using CoreEngine.GameData;
using CoreEngine.Hub;
using System;
using UnityEngine;

//public enum ItemType
//{
//    Equipment,  // 장비
//    Consumable, // 소모품
//    Material    // 재료
//}

namespace Farm.GameData.Item
{
    [System.Serializable]
    public abstract class RecordBaseItem : BaseRecord
    {
        [TableColumn, SerializeField] private string name;
        [TableColumn, SerializeField] private string description;
        [TableColumn, SerializeField] private int maxStack; // -1이면 무한대
        [TableColumn, SerializeField] private int sellPrice; // -1이면 판매불가

        

        public string Name => name;
        public string Description => description;
        public int MaxStack => maxStack;
        public int SellPrice => sellPrice;


        protected override string GetPrimaryKey() => name;
        

        //public BaseItemEffect[] GetEffect_OnUse() => effects_OnUse;
        //public BaseItemEffect[] GetEffect_OnAnimEvent() => effects_OnAnimEvent;

        //public BaseItemCondition[] GetConditions() => conditions;
        //public BaseItemConstraint[] GetConstraints() => constraints;
    }
}





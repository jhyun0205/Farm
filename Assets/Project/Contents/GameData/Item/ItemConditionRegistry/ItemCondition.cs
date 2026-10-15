using CoreEngine.Actor;
using Farm.Character;
using Farm.GameData.Item;
using UnityEngine;

namespace Farm.GameData
{
    public abstract class ItemCondition : ScriptableObject
    {
        public abstract bool IsSatisfied(BaseCharacter itemOwner, ItemRecord item);
    }
}


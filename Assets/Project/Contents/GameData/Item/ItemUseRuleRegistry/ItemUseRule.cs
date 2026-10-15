using CoreEngine.Actor;
using Farm.Character;
using Farm.GameData.Item;
using UnityEngine;

namespace Farm.GameData.Item
{
    public abstract class ItemUseRule : ScriptableObject
    {
        public abstract bool IsSatisfied(BaseCharacter itemOwner, ItemRecord item);
    }
}


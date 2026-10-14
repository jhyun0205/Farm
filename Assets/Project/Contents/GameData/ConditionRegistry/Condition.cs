using CoreEngine.Actor;
using Farm.Character;
using UnityEngine;

namespace Farm.GameData
{
    public abstract class Condition : ScriptableObject
    {
        public abstract bool IsSatisfied(BaseCharacter itemOwner);
    }
}


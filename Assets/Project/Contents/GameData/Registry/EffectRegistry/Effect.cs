using Farm.Character;
using UnityEngine;

namespace Farm.GameData
{
    public abstract class Effect : ScriptableObject
    {
        public abstract void Apply(BaseCharacter character, int[] intValues, float[] floatValues, string[] stringValues);
    }
}

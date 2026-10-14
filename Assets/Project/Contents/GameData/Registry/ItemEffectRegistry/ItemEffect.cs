using Farm.Character;
using UnityEngine;

namespace Farm.GameData.Item
{
    public abstract class ItemEffect : MonoBehaviour
    {
        public abstract void Apply(BaseCharacter character, ItemRecord item, int[] effectValue = null);
    }
}


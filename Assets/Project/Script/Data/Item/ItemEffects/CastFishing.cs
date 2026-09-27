using UnityEngine;

namespace Farm.Fishing
{
    [CreateAssetMenu(fileName = "CastFishing", menuName = "Item/ItemEffect/CastFishing")]
    public class CastFishing : BaseItemEffect
    {
        public override void ApplyEffect(BaseCharacter character, ItemDataContainer item)
        { 
            character.GetFeature<FishModule>().TryFish();
        }
    }
}

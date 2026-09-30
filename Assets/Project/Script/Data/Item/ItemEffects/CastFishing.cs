using Farm.Character;
using Farm.GameData.Item;
using UnityEngine;

namespace Farm.Fishing
{
    [CreateAssetMenu(fileName = "CastFishing", menuName = "Item/ItemEffect/CastFishing")]
    public class CastFishing : BaseItemEffect
    {
        public override void ApplyEffect(BaseCharacter character, ItemDataContainer item)
        {
            //character.GetFeature<FishModule>().TryFish();
            if(character.TryGetFeature(out FishFeture fishFeature))
            {
                fishFeature.TryFish();
            }
        }
    }
}

using CoreEngine.GameData;
using UnityEngine;

namespace Farm.GameData.Item
{
    public class ItemEffectRecord : BaseRecord
    {
        [TableColumn, SerializeField] private int id;
        [TableColumn, SerializeField] private string name;

        [TableColumn, SerializeField] private AssetId<Effect, EffectRegistry> itemEffectID;
        [TableColumn, SerializeField] private float[] effectValues;

        public void ApplyEffect()
        {
            itemEffectID.Get().Apply(null, effectValues);
        }


        protected override string GetPrimaryKey()
        {
            throw new System.NotImplementedException();
        }
    }
    public class ItemEffectTable : BaseTable<ItemEffectRecord>
    {

    }
}


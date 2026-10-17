using CoreEngine.GameData;
using Farm.GameData.Item;
using UnityEngine;

namespace Farm.GameData.Fish
{
    using FishItemFK = ForeignKey<ItemRecord, ItemTable>;

    enum Grade { Normal, Low, Mid, High, }

    [System.Serializable]
    public class FishRecord : BaseRecord
    {
        [TableColumn, SerializeField] private int id;
        [TableColumn, SerializeField] private string name;
        [TableColumn, SerializeField] private Grade grade;
        [TableColumn, SerializeField] private int weight;

        [SerializeField] private FishItemFK fishItemFK;
    }
    public class FishTable : BaseTable<FishRecord>
    {

    }
}

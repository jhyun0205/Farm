using UnityEngine;

namespace Farm.Fishing
{
    [CreateAssetMenu(fileName = "FishData", menuName = "Fishing/FishData")]
    public class FishData : BaseData_ForUi  
    {

        public Grade gradeCategory;

        [Tooltip("물고기 등급")]
        public float weight = 10f;
    }
}

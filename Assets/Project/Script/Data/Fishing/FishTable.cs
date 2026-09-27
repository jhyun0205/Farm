using System.Collections.Generic;
using UnityEngine;

namespace Farm.Fishing
{
    [CreateAssetMenu(fileName = "FishData", menuName = "Fishing/FishTable")]
    public class FishTable : ScriptableObject
    {
        public List<FishData> allFish;

        [System.Serializable]
        public class ReelGradeRule
        {
            // 릴 단계에서 나올 수 있는 등급들 자유롭게 추가 & 삭제가능
            public List<Grade> allowedGrades;
        }
        //릴 단계마다 각각의 규칙을 담는 배열 추가
        public ReelGradeRule[] reelRules = new ReelGradeRule[4];

        public List<FishData>GetPool(int reelTier)
        {
            List<FishData> pool = new List<FishData>();
            foreach(FishData fish in allFish)
            {
                //물고기 등급이 현재 릴 단계가 허용하는 등급에 포함되는지 확인
                if (reelRules[reelTier].allowedGrades.Contains(fish.gradeCategory))
                {
                    //
                    pool.Add(fish);
                }
            }
            return pool;
        }
    }
}

using System.Collections.Generic; // 지시문인 List<T>를 사용하기 위해 가져와야함
using UnityEditor;
using UnityEngine;

namespace Farm.Fishing
{
    [CustomEditor(typeof(FishTable))] 
    //Unity의 Editor를 상속받아 커스텀 인스펙터를 구현
    public class FishTableEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            FishTable table = (FishTable)target;

            if (GUILayout.Button("전체 물고기 자동 채우기"))
            {
                RefreshAllFish(table);
            }
        }

        private void RefreshAllFish(FishTable table)
        {
            string[] guids = AssetDatabase.FindAssets($"t:{nameof(FishData)}");

            List<FishData> found = new List<FishData>();

            foreach (string guid in guids)
            {
                FishData fish = AssetDatabase.LoadAssetAtPath<FishData>(AssetDatabase.GUIDToAssetPath(guid));
                if (fish != null)
                {
                    found.Add(fish);
                }
            }

            table.allFish = found;
            EditorUtility.SetDirty(table);
        }
    }
}

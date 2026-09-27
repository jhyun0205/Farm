using System;
using UnityEditor;
using UnityEngine; 

namespace Farm.Fishing 
{
    public class FishCsvConverter : BaseCsvConverter 
    {
        const string target = "Fish";

        protected override string ConverterTarget => target; 

        protected override Type TargetType => typeof(FishData);

        [MenuItem(defaultMenu + target)]  // Unity 상단 메뉴에 항목 추가
        public static void ShowWindow() => GetWindow<FishCsvConverter>("CSV Converter"); // 메뉴 클릭시 이 변환기 창을 엶

        protected override void ConvertDetails(ScriptableObject asset, int rowNum, string[] cols) 
        {
            // 형변환 부모에서 ScriptableObject를 FishData로 가져옴
            FishData asFishData = asset as FishData;
            //타입이 안맞으면 에러
            if (asFishData == null) { WarningTypeError(asset); }
            // 실제 Grade 에셋을 찾음
            Grade grade = FindGradeByName(cols[2].Trim()); 
            //리블렉션으로 gradeCategory 필드에 찾은 Grade에셋을 찾음
            asFishData.SetFieldByReflection("gradeCategory", grade); 
            //float로 변환해서 weight 필드에 설정
            asFishData.SetFieldByReflection("weight", float.Parse(cols[3].Trim()));
        }
        // 등급 문자열로 grade 에셋을 찾아서 반환
        private Grade FindGradeByName(string gradeName) 
        {
            //프로젝트에서 Grade 타입 에셋들의 고유 ID검색
            string[] guids = AssetDatabase.FindAssets($"t:{nameof(Grade)}"); 

            foreach (string guid in guids) 
            {
                //ID로 실제 경로를 구해 에셋을 불러옴
                Grade grade = AssetDatabase.LoadAssetAtPath<Grade>(AssetDatabase.GUIDToAssetPath(guid)); 
                if (grade != null && grade.gradeName == gradeName) return grade; 
            }

            Debug.LogWarning($"등급({gradeName})에 해당하는 Grade 에셋을 찾을 수 없음");
            return null; 
        }
    }
}
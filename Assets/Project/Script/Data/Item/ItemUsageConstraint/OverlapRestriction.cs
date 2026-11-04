//using CoreEngine.Actor;
//using CoreEngine.GameData;
//using Farm.Character;
//using System;
//using UnityEngine;

//// 이미 작물이 있는 곳에 중복되는거 금지
//[CreateAssetMenu(fileName = "OverlapRestriction", menuName = "Item/ItemConstraint/OverlapRestriction")]
//public class OverlapRestriction : BaseCondition
//{
//    public override bool IsSatisfied(IActorHost character)
//    {
//        CharacterTileChecker tileChecker = null;
//        if (!character.TryGetFeature(out tileChecker)) return false;

//        BoxCollider2D collider = tileChecker.GetTileMarkerBoxCollider();

//        Vector2 center = collider.bounds.center;
//        Vector2 size = collider.bounds.size;

//        // 1. "Crop"이라는 이름의 레이어 마스크를 가져옵니다.
//        // (이름의 띄어쓰기나 대소문자가 유니티 에디터 설정과 정확히 일치해야 합니다)
//        int cropLayerMask = LayerMask.GetMask("Crop");

//        // 2. 마지막 파라미터로 cropLayerMask를 넣어주면, 해당 레이어만 쏙 골라서 검사합니다.
//        Collider2D[] overlappedColliders = Physics2D.OverlapBoxAll(center, size, 0f, cropLayerMask);

//        foreach (Collider2D col in overlappedColliders)
//        {
//            if (col == collider) continue;

//            if (col.TryGetComponent(out CropViewer crop))
//            {
//                return true;
//            }
//        }

//        return false;
//    }
//}

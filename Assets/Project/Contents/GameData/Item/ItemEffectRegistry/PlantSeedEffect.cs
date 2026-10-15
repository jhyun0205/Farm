using CoreEngine.Facades;
using CoreEngine.GameData;
using CoreEngine.Helpers;
using Farm.Character;
using Farm.GameData.Crop;
using Farm.GameData.Item;
using Farm.Pool;
using UnityEngine;
using UnityEngine.TextCore.Text;

namespace Farm.GameData
{
    public class PlantSeedEffect : ItemEffect
    {
        private readonly TableHandler<CropTable, CropRecord> cropTableHandler = new();
        private ObjectPoolManager objectPoolManager;
        private ObjectPoolManager ObjectPoolManager => objectPoolManager 
            ??= CoreFacade.GetManager<ObjectPoolManager>();

        public override void Apply(BaseCharacter character, ItemRecord item, int[] effectValue = null)
        {
            CharacterTileChecker tileChecker = null;
            if (!character.TryGetFeature(out tileChecker)) return;

            CharacterCropDataSheet cropDataSheet = null;
            if (!character.TryGetFeature(out cropDataSheet)) return;

            if (!TryGetCropRecord(item, out CropRecord cropRecord)) return;

            CropViewer cropViewer = GetNewCrop(tileChecker);

            //TODO: CropDataInstance 객체 만들기

            // values를 사용해서 참조키 생성 필요함
            //int cropId = GetCropIndexFromSeed(item);
            //CropContainer cropContainer = cropDataSheet.AcquireCrop(cropId);
            //cropContainer.SetCropPosition(tileChecker.GetTilePosition());

            // view에 Model 연결
            //cropViewer?.Connect(cropContainer);
        }

        /// <summary>
        /// <see cref="TableHandler{TTable, TRecord}.TryGetReferencingRecords(uint, ulong, out System.Collections.Generic.IReadOnlyList{TRecord})"/><br/>
        /// 를 사용하여 seed아이템을 참조하는 <see cref="CropRecord"/>를 가져옴
        /// </summary>
        private bool TryGetCropRecord(ItemRecord item, out CropRecord cropRecord)
        {
            ulong seedId = (item as IIdentifiable).ID;
            if (cropTableHandler.TryGetReferencingRecords(CropTable.INDEX_SEED, seedId,
                out var result) && result is not null || result.Count != 0)
            {
                cropRecord = result[0];
                return true;
            }
            LogHelper.LogError($"씨앗 -> 작물 역참조 실패: id = {seedId}");
            cropRecord = null;
            return false;
        }

        private CropViewer GetNewCrop(CharacterTileChecker tileChecker)
        {
            var result = ObjectPoolManager.Spawn(ObjectPoolType.Crop,
                tileChecker.GetTilePosition(),
                Quaternion.identity);

            if (result is CropViewer crop) return crop;
            return null;
        }
    }
}

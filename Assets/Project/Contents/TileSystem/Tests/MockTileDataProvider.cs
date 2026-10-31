using CoreEngine.TileSystem.Contracts;
using UnityEngine;

namespace Farm.Systems.TileContent.Tests
{
    public class MockTileDataProvider : ITileDataProvider
    {
        private readonly TileCreationInfo _mockInfo;
        private readonly Sprite[] _mockSprites;

        public MockTileDataProvider(ulong tileId, BitmaskRuleType ruleType, Sprite[] sprites)
        {
            // 임의의 타일 메타 생성
            _mockInfo = new TileCreationInfo(
                tileId: tileId,
                layerIndex: 0,
                categoryMask: 1,
                connectGroupId: 100,
                ruleType: ruleType,
                maxDurability: 3);

            _mockSprites = sprites;
        }

        public bool TryGetTileCreationInfo(ulong tileId, out TileCreationInfo info)
        {
            info = _mockInfo;
            return true;
        }

        public Sprite GetTileSprite(ulong tileId, byte maskIndex)
        {
            if (_mockSprites == null || _mockSprites.Length == 0) return null;
            if (maskIndex >= _mockSprites.Length || _mockSprites[maskIndex] == null)
            {
                return _mockSprites[0];
            }
            return _mockSprites[maskIndex];
        }
    }
}
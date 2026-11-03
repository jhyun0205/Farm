using CoreEngine.TileSystem.Contracts;
using CoreEngine.TileSystem.Math;
using NUnit.Framework;
using UnityEngine;

namespace CoreEngine.TileSystem.Tests
{
    [TestFixture]
    public class TileMathTests
    {
        #region 1. GridRangeCalculator 검증

        [TestCase(0, 0, 2, 2, 2, ExpectedResult = true)]   // 사거리 모서리 끝 (포함됨)
        [TestCase(0, 0, 0, 3, 2, ExpectedResult = false)]  // X축 이탈
        [TestCase(5, 5, 5, 5, 0, ExpectedResult = true)]   // 사거리 0일 때 본인 위치
        [TestCase(5, 5, 6, 5, 0, ExpectedResult = false)]  // 사거리 0일 때 1칸 이탈
        public bool IsWithinSquareRange_ValidatesCorrectly(int cx, int cy, int tx, int ty, int range)
        {
            return GridRangeCalculator.IsWithinSquareRange(new Vector2Int(cx, cy), new Vector2Int(tx, ty), range);
        }

        [TestCase(0, 0, 5, 5, 2, 2, 2)]      // 대각선 멀리 -> (2, 2) 모서리 고정
        [TestCase(0, 0, -10, 1, 2, -2, 1)]   // 좌측 멀리 -> X축만 -2로 고정, Y축은 1 유지
        [TestCase(5, 5, 10, -5, 0, 5, 5)]    // 사거리가 0일 때 -> 항상 중심(5, 5)으로 고정
        public void GetClampedCheckerCell_ClampsToAxisAlignedSquare(int cx, int cy, int mx, int my, int range, int ex, int ey)
        {
            var center = new Vector2Int(cx, cy);
            var mouse = new Vector2Int(mx, my);
            var expected = new Vector2Int(ex, ey);

            var clamped = GridRangeCalculator.GetClampedCheckerCell(center, mouse, range);

            Assert.AreEqual(expected, clamped);
        }

        [TestCase(0, 1, ExpectedResult = 0)]   // 북쪽
        [TestCase(1, 0, ExpectedResult = 2)]   // 동쪽
        [TestCase(0, -1, ExpectedResult = 4)]  // 남쪽
        [TestCase(-1, 0, ExpectedResult = 6)]  // 서쪽
        public int GetClockwiseOctantIndex_ReturnsCorrectIndex(int dx, int dy)
        {
            return GridRangeCalculator.GetClockwiseOctantIndex(Vector2Int.zero, new Vector2Int(dx, dy));
        }

        #endregion

        #region 2. TileBitmaskCalculator 검증

        [Test]
        public void GetOffset_ReturnsCorrectDirectionVector()
        {
            Assert.AreEqual(new Vector2Int(0, 1), TileBitmaskCalculator.GetOffset(GridDirection.North));
            Assert.AreEqual(new Vector2Int(1, -1), TileBitmaskCalculator.GetOffset(GridDirection.SouthEast));
        }

        [TestCase(85, ExpectedResult = 15)] // 85 (01010101) N, E, S, W 모두 켜짐 -> 15 (00001111)로 압축
        [TestCase(1, ExpectedResult = 1)]   // 북쪽(1)만 -> 1
        [TestCase(64, ExpectedResult = 8)]  // 서쪽(64)만 -> 8
        [TestCase(255, ExpectedResult = 15)]// 모든 비트(255) 켜짐 -> 대각선 무시하고 15로 압축
        public byte Cardinal4_CompressesTo0To15_And_IgnoresDiagonals(byte rawMask)
        {
            return TileBitmaskCalculator.ComputeMaskIndex(BitmaskRuleType.Cardinal4, rawMask);
        }

        [Test]
        public void Blob8_AppliesDiagonalGatingCorrectly()
        {
            // 케이스 A: 직교 타일(N, E)이 모두 있어서 대각선(NE)이 유지되는 경우
            // N(1) + NE(2) + E(4) = 7
            byte validDiagonal = 7;
            Assert.AreEqual(7, TileBitmaskCalculator.ApplyDiagonalGating(validDiagonal));

            // 케이스 B: 북쪽(N)만 있고 동쪽(E)이 없는데 북동쪽(NE) 대각선만 있는 경우
            // N(1) + NE(2) = 3  -> 게이팅 발동! NE(2)가 잘려나가고 N(1)만 남아야 함
            byte invalidDiagonal = 3;
            Assert.AreEqual(1, TileBitmaskCalculator.ApplyDiagonalGating(invalidDiagonal));
        }

        [Test]
        public void Blob8_ComputeMaskIndex_AlwaysReturnsWithin0To46()
        {
            // 256가지의 모든 쓰레기 값이 들어와도 Blob47LookupTable 덕분에 0~46 사이로 압축되는지 확인
            for (int i = 0; i < 256; i++)
            {
                byte result = TileBitmaskCalculator.ComputeMaskIndex(BitmaskRuleType.Blob8, (byte)i);
                Assert.IsTrue(result >= 0 && result <= 46, $"입력값 {i}가 범위(0~46)를 벗어난 {result}를 반환했습니다.");
            }
        }

        #endregion
    }
}
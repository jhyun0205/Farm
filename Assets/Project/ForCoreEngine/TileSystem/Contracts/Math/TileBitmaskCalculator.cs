using CoreEngine.TileSystem.Contracts;
using UnityEngine;

namespace CoreEngine.TileSystem.Math
{
    /// <summary>
    /// 4방향(16타일) 및 8방향 대각선 게이팅(47타일 Blob) 비트마스크 계산기
    /// </summary>
    public static class TileBitmaskCalculator
    {
        // 시계방향 순회 8방향 오프셋: 북(0)부터 시작하여 짝수는 직교, 홀수는 대각선
        public static readonly Vector2Int[] NeighborOffsets = new Vector2Int[8]
        {
            new Vector2Int( 0,  1), // 0: 북 (N)
            new Vector2Int( 1,  1), // 1: 북동 (NE)
            new Vector2Int( 1,  0), // 2: 동 (E)
            new Vector2Int( 1, -1), // 3: 남동 (SE)
            new Vector2Int( 0, -1), // 4: 남 (S)
            new Vector2Int(-1, -1), // 5: 남서 (SW)
            new Vector2Int(-1,  0), // 6: 서 (W)
            new Vector2Int(-1,  1)  // 7: 북서 (NW)
        };

        // 8방향 원시 비트마스크(0~255)를 스프라이트 배열 인덱스(0~46)로 직관적으로 압축해주는 정적 룩업 테이블
        // (실제 프로젝트 빌드 시 데이터 크기에 맞춰 초기화되거나 유니티 에디터 스크립트에서 주입 가능)
        private static readonly byte[] Blob47LookupTable = InitializeBlob47Lookup();

        private static byte[] InitializeBlob47Lookup()
        {
            byte[] table = new byte[256];
            // 대각선 게이팅을 거친 유효한 패턴 47개에 대해 0~46 순차 인덱스 할당
            // (구현 단계에서 정적 배열이나 에디터 제너레이터로 바인딩될 47개 매핑 영역)
            for (int i = 0; i < 256; i++)
            {
                table[i] = 0; // 기본값
            }
            return table;
        }

        /// <summary>
        /// 룰 타입과 주변 8방향 원시 비트 상태를 받아 압축된 연속 스프라이트 인덱스를 반환
        /// </summary>
        public static byte ComputeMaskIndex(BitmaskRuleType ruleType, byte raw8BitMask)
        {
            if (ruleType == BitmaskRuleType.None) return 0;

            // 4방향 모드 (Cardinal4: 0 ~ 15)
            if (ruleType == BitmaskRuleType.Cardinal4)
            {
                bool n = (raw8BitMask & (1 << 0)) != 0;
                bool e = (raw8BitMask & (1 << 2)) != 0;
                bool s = (raw8BitMask & (1 << 4)) != 0;
                bool w = (raw8BitMask & (1 << 6)) != 0;

                return (byte)((n ? 1 : 0) | (e ? 2 : 0) | (s ? 4 : 0) | (w ? 8 : 0));
            }

            // 8방향 대각선 게이팅 모드 (Blob8: 0 ~ 46)
            // 양옆 직교 타일이 모두 켜져 있을 때만 대각선 비트 인정
            bool cn = (raw8BitMask & (1 << 0)) != 0;
            bool ce = (raw8BitMask & (1 << 2)) != 0;
            bool cs = (raw8BitMask & (1 << 4)) != 0;
            bool cw = (raw8BitMask & (1 << 6)) != 0;

            bool ne = ((raw8BitMask & (1 << 1)) != 0) && cn && ce;
            bool se = ((raw8BitMask & (1 << 3)) != 0) && cs && ce;
            bool sw = ((raw8BitMask & (1 << 5)) != 0) && cs && cw;
            bool nw = ((raw8BitMask & (1 << 7)) != 0) && cn && cw;

            int gatedMask = (cn ? 1 : 0)
                          | (ne ? 2 : 0)
                          | (ce ? 4 : 0)
                          | (se ? 8 : 0)
                          | (cs ? 16 : 0)
                          | (sw ? 32 : 0)
                          | (cw ? 64 : 0)
                          | (nw ? 128 : 0);

            return Blob47LookupTable[gatedMask];
        }
    }
}
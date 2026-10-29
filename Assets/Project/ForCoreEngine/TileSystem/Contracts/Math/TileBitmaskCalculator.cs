using CoreEngine.TileSystem.Contracts;
using UnityEngine;

namespace CoreEngine.TileSystem.Math
{
    /// <summary>
    /// 4방향(16타일) 및 8방향 대각선 게이팅(47타일 Blob) 비트마스크 계산기
    /// </summary>
    public static class TileBitmaskCalculator
    {
        private const byte BIT_N    = 1 << 0; // 1   (0000_0001)
        private const byte BIT_NE   = 1 << 1; // 2   (0000_0010)
        private const byte BIT_E    = 1 << 2; // 4   (0000_0100)
        private const byte BIT_SE   = 1 << 3; // 8   (0000_1000)
        private const byte BIT_S    = 1 << 4; // 16  (0001_0000)
        private const byte BIT_SW   = 1 << 5; // 32  (0010_0000)
        private const byte BIT_W    = 1 << 6; // 64  (0100_0000)
        private const byte BIT_NW   = 1 << 7; // 128 (1000_0000)

        // 직교 4방향 일괄 추출 마스크 (0101_0101 = 85)
        private const byte BITMASK_NESW = BIT_N | BIT_E | BIT_S | BIT_W;

        // 대각선 게이팅 검증용 복합 마스크 (양옆 직교 2방향 + 해당 대각선 1방향)
        private const byte BITMASK_NE = BIT_N | BIT_E | BIT_NE;
        private const byte BITMASK_SE = BIT_E | BIT_S | BIT_SE;
        private const byte BITMASK_SW = BIT_S | BIT_W | BIT_SW;
        private const byte BITMASK_NW = BIT_W | BIT_N | BIT_NW;

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

        // 8방향 게이팅 비트마스크(0~255)를 스프라이트 배열 인덱스(0~46)로 압축하는 룩업 테이블
        private static readonly byte[] Blob47LookupTable = InitializeBlob47Lookup();

        /// <summary>
        /// 외부에서 명시적인 Enum을 사용해 특정 방향의 오프셋을 안전하게 가져오기
        /// </summary>
        public static Vector2Int GetOffset(GridDirection direction)
        {
            return NeighborOffsets[(int)direction];
        }

        /// <summary>
        /// 룰 타입과 주변 8방향 원시 비트 상태를 받아 압축된 연속 스프라이트 인덱스를 반환
        /// </summary>
        public static byte ComputeMaskIndex(BitmaskRuleType ruleType, byte raw8BitMask)
        {
            if (ruleType == BitmaskRuleType.None) return 0;

            // 4방향 모드 (Cardinal4: 0 ~ 15 연속 인덱스 반환)
            // 짝수 자리(0, 2, 4, 6번 비트)에 퍼져 있는 직교 비트를 우측 시프트하여 0, 1, 2, 3번 비트로 압축
            if (ruleType == BitmaskRuleType.Cardinal4)
            {
                return (byte)(
                    (raw8BitMask & BIT_N) |
                    ((raw8BitMask & BIT_E) >> 1) |
                    ((raw8BitMask & BIT_S) >> 2) |
                    ((raw8BitMask & BIT_W) >> 3)
                );
            }

            // 8방향 대각선 게이팅 모드 (Blob8: 0 ~ 46 연속 인덱스 반환)
            byte gatedMask = ApplyDiagonalGating(raw8BitMask);
            return Blob47LookupTable[gatedMask];
        }

        /// <summary>
        /// 양옆 직교 타일이 모두 켜져 있을 때만 대각선 비트를 인정하는 게이팅 필터
        /// </summary>
        public static byte ApplyDiagonalGating(byte raw8BitMask)
        {
            int nesw = raw8BitMask & BITMASK_NESW;

            bool ne = (raw8BitMask & BITMASK_NE) == BITMASK_NE;
            bool se = (raw8BitMask & BITMASK_SE) == BITMASK_SE;
            bool sw = (raw8BitMask & BITMASK_SW) == BITMASK_SW;
            bool nw = (raw8BitMask & BITMASK_NW) == BITMASK_NW;

            return (byte)(nesw
                        | (ne ? BIT_NE : 0)
                        | (se ? BIT_SE : 0)
                        | (sw ? BIT_SW : 0)
                        | (nw ? BIT_NW : 0));
        }

        private static byte[] InitializeBlob47Lookup()
        {
            byte[] table = new byte[256];
            byte nextIndex = 0;

            // 1차 패스: 게이팅 전후 값이 동일한 '순수 유효 패턴(정확히 47개)'에 0 ~ 46 인덱스를 순차 부여
            for (int i = 0; i < 256; i++)
            {
                byte raw = (byte)i;
                if (ApplyDiagonalGating(raw) == raw)
                {
                    table[i] = nextIndex++;
                }
            }

            // 2차 패스: 게이팅으로 잘려나갈 비트가 섞인 나머지 입력값들도 게이팅된 결과 인덱스로 안전하게 매핑
            for (int i = 0; i < 256; i++)
            {
                byte gated = ApplyDiagonalGating((byte)i);
                table[i] = table[gated];
            }

            return table;
        }
    }
}
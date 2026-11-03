using UnityEngine;

namespace CoreEngine.TileSystem.Math
{
    public static class TileDirections
    {
        /// <summary>
        /// 북(N)쪽부터 시작하는 시계방향 8방향 오프셋 배열
        /// </summary>
        public static readonly Vector2Int[] Clockwise8 =
        {
            new Vector2Int(0, 1),   // North     (1 << 0)
            new Vector2Int(1, 1),   // NorthEast (1 << 1)
            new Vector2Int(1, 0),   // East      (1 << 2)
            new Vector2Int(1, -1),  // SouthEast (1 << 3)
            new Vector2Int(0, -1),  // South     (1 << 4)
            new Vector2Int(-1, -1), // SouthWest (1 << 5)
            new Vector2Int(-1, 0),  // West      (1 << 6)
            new Vector2Int(-1, 1)   // NorthWest (1 << 7)
        };
    }
}
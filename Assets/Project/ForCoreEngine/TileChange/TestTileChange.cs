using NUnit.Framework.Internal;
using System.Threading;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

namespace Farm.TileSystem.Test
{
    public enum StartSeason
    {
        Spring,
        Summer,
        Fall,
        Winter,
    }
    public class TestTileChange : MonoBehaviour
    {
        [Header("Tile Opntion")]
        public Tilemap tilemap;
        public Tile LevelTile; // 파일 에셋으로 존재하는 타일
        private Tile RunTimeTile; // 런타임에 생성해서 Sprite를 변경할 타일

        public StartSeason startSeason;


        [Header("Seasonal Buttons")]
        public Button SpringButton;
        public Button SummerButton;
        public Button FallButton;
        public Button WinterButton;

        [Header("Seasonal Sprites")]
        public Sprite Spring;
        public Sprite Summer;
        public Sprite Fall;
        public Sprite Winter;

        

        private void Awake()
        {
            RunTimeTile = new();
            RunTimeTile.sprite = LevelTile.sprite;
            tilemap.SwapTile(LevelTile, RunTimeTile);

            SpringButton.onClick.AddListener(() => ChangeTileSprite(Spring));
            SummerButton.onClick.AddListener(() => ChangeTileSprite(Summer));
            FallButton.onClick.AddListener(() => ChangeTileSprite(Fall));
            WinterButton.onClick.AddListener(() => ChangeTileSprite(Winter));

            switch (startSeason)
            {
                case StartSeason.Spring:
                    ChangeTileSprite(Spring);
                    break;

                case StartSeason.Summer:
                    ChangeTileSprite(Summer);
                    break;

                case StartSeason.Fall:
                    ChangeTileSprite(Fall);
                    break;

                case StartSeason.Winter:
                    ChangeTileSprite(Winter);
                    break;
            }
        }

        public void ChangeTileSprite(Sprite newSprite)
        {
            RunTimeTile.sprite = newSprite;
            tilemap.RefreshAllTiles();
        }
    }
}


using CoreEngine.EventBus;
using CoreEngine.TileSystem.Contracts;
using Farm.GameData.Tile; // 수정된 TileDataRegistry 위치
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Farm.Systems.TileContent
{
    public class TileGridRenderer : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Tilemap _tilemap;

        private TileDataProvider _tileDataRegistry;

        public void Initialize(TileDataProvider dataRegistry)
        {
            _tileDataRegistry = dataRegistry;
        }

        private void OnEnable()
        {
            EventBus<TileChangedEvent>.Subscribe(OnTileChanged);
        }

        private void OnDisable()
        {
            EventBus<TileChangedEvent>.Unsubscribe(OnTileChanged);
        }

        private void OnTileChanged(TileChangedEvent evt)
        {
            if (_tilemap == null || _tileDataRegistry == null) return;

            Vector3Int position = new Vector3Int(evt.CellPos.x, evt.CellPos.y, 0);

            if (evt.TileId == 0)
            {
                _tilemap.SetTile(position, null);
                return;
            }

            Sprite sprite = _tileDataRegistry.GetTileSprite(evt.TileId, evt.MaskIndex);
            if (sprite != null)
            {
                Tile tile = ScriptableObject.CreateInstance<Tile>();
                tile.sprite = sprite;
                _tilemap.SetTile(position, tile);
            }
            else
            {
                _tilemap.SetTile(position, null);
            }
        }
    }
}
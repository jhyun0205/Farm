using System.Collections.Generic;
using CoreEngine.EventBus;
using CoreEngine.TileSystem.Contracts;
using Farm.GameData.Tile;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Farm.Systems.TileContent
{
    public class TileGridRenderer : MonoBehaviour
    {
        [Header("Layer Tilemaps (Index = LayerIndex)")]
        [SerializeField] private Tilemap[] _layerTilemaps;

        private TileDataProvider _dataProvider;
        private readonly Dictionary<Sprite, Tile> _tileInstanceCache = new(128);

        public void Initialize(TileDataProvider dataProvider)
        {
            _dataProvider = dataProvider;
        }

        private void OnEnable()
        {
            EventBus<TileChangedEvent>.Subscribe(OnTileChanged);
        }

        private void OnDisable()
        {
            EventBus<TileChangedEvent>.Unsubscribe(OnTileChanged);
        }

        private void OnDestroy()
        {
            foreach (Tile cachedTile in _tileInstanceCache.Values)
            {
                if (cachedTile != null)
                {
                    Destroy(cachedTile);
                }
            }
            _tileInstanceCache.Clear();
        }

        private void OnTileChanged(TileChangedEvent evt)
        {
            if (_dataProvider == null || _layerTilemaps == null) return;
            if (evt.LayerIndex >= _layerTilemaps.Length) return;

            Tilemap targetTilemap = _layerTilemaps[evt.LayerIndex];
            if (targetTilemap == null) return;

            Vector3Int position = new Vector3Int(evt.CellPos.x, evt.CellPos.y, 0);

            if (evt.TileId == 0)
            {
                targetTilemap.SetTile(position, null);
                return;
            }

            Sprite sprite = _dataProvider.GetTileSprite(evt.TileId, evt.MaskIndex);
            if (sprite == null)
            {
                targetTilemap.SetTile(position, null);
                return;
            }

            if (!_tileInstanceCache.TryGetValue(sprite, out Tile tileAsset))
            {
                tileAsset = ScriptableObject.CreateInstance<Tile>();
                tileAsset.hideFlags = HideFlags.DontSave;
                tileAsset.sprite = sprite;
                _tileInstanceCache[sprite] = tileAsset;
            }

            targetTilemap.SetTile(position, tileAsset);
        }
    }
}
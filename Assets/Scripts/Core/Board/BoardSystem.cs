using Core.Board.Abstract;
using Core.Board.Tile.Abstract;
using Core.Board.Tile.Interface;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Core.Board
{
    /// <summary>
    /// Represents the main implementation of a board system. Handles board creation, tile instantiation,
    /// and board lifecycle management. It derives from the abstract BoardSystemBase class.
    /// </summary>
    public class BoardSystem : BoardSystemBase
    {
        private readonly TileBase _tilePrefab;

        public BoardSystem(int rowCount, int columnCount, TileBase tilePrefab) : base(rowCount, columnCount)
        {
            _tilePrefab = tilePrefab;
        }
        
        public override void Initialize()
        {
            if (_tilePrefab == null)
            {
                Debug.LogWarning("Tile prefab is null, will not initialize board");
                return;
            }
            
            for (var i = 0; i < RowCount; i++)
            {
                for (var j = 0; j < ColumnCount; j++)
                {
                    ITile tile = Object.Instantiate(_tilePrefab, new Vector3(j, -i, 0), Quaternion.identity);
                    tile.Initialize(i, j);
                    Tiles[i, j] = tile;
                }
            }
        }

        public override void Dispose()
        {
            for (var i = 0; i < RowCount; i++)
            {
                for (var j = 0; j < ColumnCount; j++)
                {
                    var tile = Tiles[i, j];
                    if (tile == null)
                    {
                        continue;
                    }
                    tile.Destroy();
                }
            }
        }
    }
}

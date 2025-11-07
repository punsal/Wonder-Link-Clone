using Board.Abstract;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Board
{
    public class GameBoard : BoardBase
    {
        private readonly Tile _tilePrefab;

        public GameBoard(int rowCount, int columnCount, Tile tilePrefab) : base(rowCount, columnCount)
        {
            _tilePrefab = tilePrefab;
        }
        
        public override void Initialize()
        {
            for (var i = 0; i < RowCount; i++)
            {
                for (var j = 0; j < ColumnCount; j++)
                {
                    var tile = Object.Instantiate(_tilePrefab, new Vector3(j, -i, 0), Quaternion.identity);
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
                    tile.Dispose();
                }
            }
        }
    }
}

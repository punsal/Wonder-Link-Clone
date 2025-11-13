using UnityEngine;

namespace Core.Board.Tile.Interface
{
    public interface ITile
    {
        string Name { get; }
        int Row { get; }
        int Column { get; }
        Vector3 Position { get; }
        void Initialize(int row, int column);
        void Destroy();
        void Highlight();
        void Conceal();
    }
}
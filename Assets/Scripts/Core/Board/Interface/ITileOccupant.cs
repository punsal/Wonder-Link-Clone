using Core.Board.Tile.Interface;

namespace Core.Board.Interface
{
    /// <summary>
    /// Represents an object that can occupy a tile on a game board.
    /// </summary>
    public interface ITileOccupant
    {
        ITile Tile { get; }
        void Occupy(ITile tile);
        void Release();
    }
}
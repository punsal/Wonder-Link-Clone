using Core.Board.Abstract;

namespace Core.Board.Interface
{
    /// <summary>
    /// Represents an object that can occupy a tile on a game board.
    /// </summary>
    public interface ITileOccupant
    {
        TileBase Tile { get; }
        // ReSharper disable once UnusedMemberInSuper.Global
        void Occupy(TileBase tile);
        // ReSharper disable once UnusedMemberInSuper.Global
        void Release();
    }
}
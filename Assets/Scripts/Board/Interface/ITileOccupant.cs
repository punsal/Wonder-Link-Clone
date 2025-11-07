using Board.Abstract;

namespace Board.Interface
{
    public interface ITileOccupant
    {
        TileBase Tile { get; }
        void Occupy(TileBase tile);
        void Release();
    }
}
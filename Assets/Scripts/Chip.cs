using Board.Abstract;
using Board.Interface;
using UnityEngine;

public class Chip : MonoBehaviour, ITileOccupant
{
    public TileBase Tile { get; private set; }

    public void Occupy(TileBase tile)
    {
        if (Tile != null)
        {
            Debug.LogError("Chip already has a tile");
            return;
        }
        
        Tile = tile;
        Move(tile.Position);
    }

    public void Release()
    {
        Tile = null;
    }

    private void Move(Vector3 position)
    {
        transform.position = position;
    }
}

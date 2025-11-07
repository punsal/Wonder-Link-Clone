using System;
using System.Collections.Generic;
using System.Linq;
using Board.Interface;
using UnityEngine;

namespace Board.Abstract
{
    public abstract class BoardBase : IDisposable
    {
        protected int RowCount { get; }
        protected int ColumnCount { get; }
        protected TileBase[,] Tiles { get; }
        private List<ITileOccupant> _occupants;

        protected BoardBase(int rowCount, int columnCount)
        {
            RowCount = rowCount;
            ColumnCount = columnCount;
            Tiles = new TileBase[rowCount, columnCount];
            _occupants = new List<ITileOccupant>();
        }

        public abstract void Initialize();
        public abstract void Dispose();

        public bool TryGetEmptyTile(out TileBase tile)
        {
            tile = null;
    
            // Use HashSet for O(1) lookup instead of O(n) Contains
            var occupiedTiles = new HashSet<TileBase>(
                _occupants
                    .Select(occupant => occupant.Tile)
                    .Where(t => t != null)
            );

            // Find the first empty tile without creating a full list
            for (var i = 0; i < RowCount; i++)
            {
                for (var j = 0; j < ColumnCount; j++)
                {
                    var currentTile = Tiles[i, j];
                    if (currentTile == null || occupiedTiles.Contains(currentTile))
                    {
                        continue;
                    }
                    tile = currentTile;
                    return true;
                }
            }

            return false;
        }

        public void AddOccupant(ITileOccupant occupant)
        {
            if (occupant == null)
            {
                Debug.LogError("Occupant cannot be null");
                return;
            }
            
            if (_occupants.Contains(occupant))
            {
                Debug.LogError("Occupant already exists");
                return;
            }

            if (occupant.Tile == null)
            {
                Debug.LogError("Occupant does not have a tile");
                return;
            }
            
            // Validate tile belongs to this board
            if (!IsTileInBoard(occupant.Tile))
            {
                Debug.LogError($"Tile at ({occupant.Tile.Row}, {occupant.Tile.Column}) does not belong to this board");
                return;
            }

            _occupants.Add(occupant);
        }
        
        // Defensive check to assure that the tile is in the board
        private bool IsTileInBoard(TileBase tile)
        {
            if (tile.Row < 0 || tile.Row >= RowCount || tile.Column < 0 || tile.Column >= ColumnCount)
                return false;
        
            return Tiles[tile.Row, tile.Column] == tile;
        }

        public void RemoveOccupant(ITileOccupant occupant)
        {
            if (occupant == null)
            {
                Debug.LogError("Occupant cannot be null");
                return;
            }

            if (!_occupants.Contains(occupant))
            {
                Debug.LogWarning("Occupant does not exist");
                return;
            }

            // Fixed: occupant should have released its tile before removal
            if (occupant.Tile == null)
            {
                _occupants.Remove(occupant);
            }
            else
            {
                Debug.LogError($"Occupant still has a tile at ({occupant.Tile.Row}, {occupant.Tile.Column}). Call Release() first.");
            }
        }
    }
}
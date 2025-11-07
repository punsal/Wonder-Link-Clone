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
            var tiles = Tiles.Cast<TileBase>().ToList();

            foreach (var occupantTile in _occupants
                         .Select(occupant => occupant.Tile)
                         .Where(occupantTile => tiles.Contains(occupantTile)))
            {
                tiles.Remove(occupantTile);
            }
            
            if (tiles.Count == 0)
            {
                return false;
            }

            var availableTiles = tiles.OrderBy(t => t.Row).ThenBy(t => t.Column);
            tile = availableTiles.First();
            return true;
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
            
            _occupants.Add(occupant);
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

            if (occupant.Tile != null)
            {
                Debug.LogError("Occupant has a tile");
                return;
            }

            _occupants.Remove(occupant);
        }
    }
}
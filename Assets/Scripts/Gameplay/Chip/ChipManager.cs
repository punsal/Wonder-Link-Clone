using System.Collections.Generic;
using System.Linq;
using Core.Board.Abstract;
using Core.Board.Tile.Interface;
using Gameplay.Chip.Abstract;
using UnityEngine;

namespace Gameplay.Chip
{
    /// <summary>
    /// Manages the spawning and destruction of chips within a board system.
    /// </summary>
    public class ChipManager : ChipManagerBase
    {
        private readonly List<ChipBase> _activeChips;

        public override IReadOnlyList<ChipBase> ActiveChips => _activeChips;

        public ChipManager(BoardSystemBase boardSystem, List<ChipBase> chipPrefabs) : base(boardSystem, chipPrefabs)
        {
            _activeChips = new List<ChipBase>();
        }

        public override void FillBoard()
        {
            while (BoardSystem.TryGetEmptyTile(out var tile))
            {
                SpawnRandomChipAt(tile);
            }
        }

        protected override ChipBase SpawnChipAt(ITile tile, ChipBase chipPrefab)
        {
            var existingChip = _activeChips.FirstOrDefault(c => c != null && c.Tile == tile);
            if (existingChip != null)
            {
                Debug.LogError($"Tile at ({tile.Row}, {tile.Column}) is already occupied by {existingChip.name}!");
                return existingChip;
            }
            
            var spawnPosition = new Vector3(
                tile.Position.x, 
                tile.Position.y + BoardSystem.RowCount, 
                0);
            
            var chip = Object.Instantiate(chipPrefab, spawnPosition, Quaternion.identity);
            chip.name = $"Chip_{tile.Row}_{tile.Column}";

            chip.Occupy(tile);
            BoardSystem.AddOccupant(chip);
            _activeChips.Add(chip);
            Debug.Log($"Spawned chip at ({tile.Row}, {tile.Column}). Total active: {_activeChips.Count}");

            chip.MoveTo(tile.Position, 0.2f);
            
            return chip;
        }

        protected override void DestroyChip(ChipBase chip)
        {
            if (chip == null)
            {
                Debug.LogWarning("Cannot destroy null chip");
                return;
            }

            Debug.Log($"Destroying chip {chip.name} at ({chip.Tile?.Row}, {chip.Tile?.Column}). Before: {_activeChips.Count}");
            
            _activeChips.Remove(chip);
            chip.Release();
            BoardSystem.RemoveOccupant(chip);
            Object.Destroy(chip.gameObject);
            
            Debug.Log($"After destroy: {_activeChips.Count}");
        }

        protected override void DestroyAllChips()
        {
            // Iterate backwards to avoid issues with list modification
            for (var i = _activeChips.Count - 1; i >= 0; i--)
            {
                var chip = _activeChips[i];
                if (chip == null)
                {
                    continue;
                }

                chip.Release();
                BoardSystem.RemoveOccupant(chip);
                Object.Destroy(chip.gameObject);
            }

            _activeChips.Clear();
        }

        protected override void CleanupDestroyedChips()
        {
            var nullCount = _activeChips.RemoveAll(chip => chip == null);
            if (nullCount > 0)
            {
                Debug.LogWarning($"Cleaned up {nullCount} null references");
            }
            
            Debug.Log($"After cleanup: {_activeChips.Count} active chips");
        }
    }
}
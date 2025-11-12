using System;
using System.Collections.Generic;
using System.Linq;
using Core.Board.Abstract;
using Core.Board.Tile.Interface;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Gameplay.Chip.Abstract
{
    /// <summary>
    /// Provides a base implementation for managing gameplay chips in a board system.
    /// This abstract class handles the spawning, destruction, and querying of chips.
    /// </summary>
    public abstract class ChipManagerBase : IDisposable
    {
        protected readonly BoardSystemBase BoardSystem;
        private readonly List<ChipBase> _chipPrefabs;

        public abstract IReadOnlyList<ChipBase> ActiveChips { get; }

        protected ChipManagerBase(BoardSystemBase boardSystem, List<ChipBase> chipPrefabs)
        {
            BoardSystem = boardSystem;
            _chipPrefabs = chipPrefabs ?? new List<ChipBase>();
        }

        public void Dispose()
        {
            DestroyAllChips();
        }

        // Spawning
        public abstract void FillBoard();
        protected abstract ChipBase SpawnChipAt(ITile tile, ChipBase chipPrefab);
        public void SpawnRandomChipAt(ITile tile)
        {
            var randomPrefab = _chipPrefabs[Random.Range(0, _chipPrefabs.Count)];
            SpawnChipAt(tile, randomPrefab);
        }

        // Destruction
        protected abstract void DestroyChip(ChipBase chip);
        protected abstract void DestroyAllChips();
        protected abstract void CleanupDestroyedChips();
        public void DestroyChips(IEnumerable<ChipBase> chips)
        {
            // cache to modify and iterate
            var chipList = chips.ToList();
            
            Debug.Log($"Destroying {chipList.Count} chips. Current active: {ActiveChips.Count}");
            
            foreach (var chip in chipList)
            {
                DestroyChip(chip);
            }
            
            CleanupDestroyedChips();
        }

        // Queries
        public ChipBase FindChipAt(int row, int col)
        {
            ChipBase foundChip = null;
            var foundCount = 0;
            
            // Don't use LINQ here, it's too slow
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var chip in ActiveChips)
            {
                if (!chip || chip.Tile == null || chip.Tile.Row != row || chip.Tile.Column != col)
                {
                    continue;
                }
                foundChip = chip;
                foundCount++;
            }
            
            if (foundCount > 1)
            {
                Debug.LogError($"Multiple chips ({foundCount}) found at ({row}, {col})!");
            }

            return foundChip;
        }
    }
}
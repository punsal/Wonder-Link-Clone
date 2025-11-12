using System.Collections;
using System.Linq;
using Core.Board.Abstract;
using Core.Runner.Interface;
using Gameplay.Chip.Abstract;
using Gameplay.Systems.Shuffle.Abstract;
using UnityEngine;

namespace Gameplay.Systems.Shuffle
{
    /// <summary>
    /// Shuffles chips on the board by swapping their positions randomly.
    /// Includes visual animation for the shuffle process.
    /// </summary>
    public class ShuffleSystem : ShuffleSystemBase
    {
        public ShuffleSystem(
            BoardSystemBase boardSystem, 
            ChipManagerBase chipManager,
            ICoroutineRunner coroutineRunner) 
            : base(boardSystem, chipManager, coroutineRunner)
        {
            // empty
        }

        protected override IEnumerator Shuffle()
        {
            var chips = ChipManager.ActiveChips
                .Where(c => c != null && c.Tile != null).
                ToList();
            
            Debug.Log($"Shuffling {chips.Count} chips");

            if (chips.Count == 0)
            {
                Debug.LogWarning("No chips to shuffle");
                yield break;
            }
            
            var expectedChipCount = BoardSystem.RowCount * BoardSystem.ColumnCount;
            if (chips.Count != expectedChipCount)
            {
                Debug.LogWarning($"Chip count mismatch! Expected: {expectedChipCount}, Got: {chips.Count}");
                var duplicates = chips
                    .GroupBy(c => (c.Tile.Row, c.Tile.Column))
                    .Where(g => g.Count() > 1)
                    .ToList();
                
                if (duplicates.Any())
                {
                    Debug.LogError($"Found {duplicates.Count} duplicate positions:");
                    foreach (var dup in duplicates)
                    {
                        Debug.LogError($"Position ({dup.Key.Row}, {dup.Key.Column}): {dup.Count()} chips");
                    }
                }
            }
            
            var originalTiles = chips.Select(c => c.Tile).ToList();
            
            foreach (var chip in chips)
            {
                chip.Release();
                BoardSystem.RemoveOccupant(chip);
            }

            var shuffledTiles = originalTiles.OrderBy(x => Random.value).ToList();

            for (var i = 0; i < chips.Count; i++)
            {
                var chip = chips[i];
                var newTile = shuffledTiles[i];

                chip.Occupy(newTile);
                BoardSystem.AddOccupant(chip);
                
                chip.MoveTo(newTile.Position, 0.3f);
                
                yield return null;
            }

            yield return new WaitForSeconds(0.35f);
        }
    }
}
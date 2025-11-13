using System.Collections;
using System.Collections.Generic;
using Core.Board.Abstract;
using Core.Runner.Interface;
using Gameplay.Chip.Abstract;
using Gameplay.Systems.BoardRefill.Abstract;
using Gameplay.Systems.MatchDetection;
using Gameplay.Systems.MatchDetection.Abstract;
using UnityEngine;

namespace Gameplay.Systems.BoardRefill
{
    /// <summary>
    /// Represents a system responsible for refilling a board with chips, using a coroutine-based
    /// mechanism for performing the refill operations in sequential steps such as destroying chips,
    /// applying gravity, and spawning new chips.
    /// </summary>
    public class BoardRefillSystem : BoardRefillSystemBase
    {
        protected override MatchDetectionSystemBase MatchDetectionSystem { get; }

        public BoardRefillSystem(BoardSystemBase boardSystem, ChipManagerBase chipManager, ICoroutineRunner coroutineRunner) : base(boardSystem, chipManager, coroutineRunner)
        {
            MatchDetectionSystem = new MatchDetectionSystem(boardSystem, chipManager);
        }

        protected override IEnumerator Refill(List<ChipBase> chips)
        {
            yield return CoroutineRunner.StartCoroutine(DestroyChips(chips));
            yield return CoroutineRunner.StartCoroutine(ApplyGravity());
            yield return CoroutineRunner.StartCoroutine(SpawnNewChips());
        }

        private IEnumerator DestroyChips(List<ChipBase> chips)
        {
            // Visual effects
            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (var chip in chips)
            {
                if (chip)
                {
                    chip.Destroy();
                }
            }
        
            // Wait for effects
            yield return new WaitForSeconds(0.3f);
        
            // Actual destruction
            ChipManager.DestroyChips(chips);
        
            yield return null;
        }

        private IEnumerator ApplyGravity()
        {
            var columnCount = BoardSystem.ColumnCount;
            var rowCount = BoardSystem.RowCount;
            
            var anyChipMoved = false;
        
            // Process each column from left to right (col: 0 -> N)
            for (var col = 0; col < columnCount; col++)
            {
                // Process from bottom to top (row: N -> 0)
                for (var row = rowCount - 1; row >= 0; row--)
                {
                    var chipAtPosition = ChipManager.FindChipAt(row, col);

                    if (!chipAtPosition)
                    {
                        continue;
                    }
                    var targetRow = FindLowestEmptyRow(row, col);

                    if (targetRow == row)
                    {
                        continue;
                    }
                    yield return CoroutineRunner.StartCoroutine(MoveChipToTile(chipAtPosition, targetRow, col));
                    anyChipMoved = true;
                }
            }
        
            if (anyChipMoved)
            {
                yield return new WaitForSeconds(0.2f);
            }
        }
        
        private int FindLowestEmptyRow(int startRow, int col)
        {
            var rowCount = BoardSystem.RowCount;
            var lowestEmpty = startRow;
        
            // Check all rows below
            for (var row = startRow + 1; row < rowCount; row++)
            {
                var chipBelow = ChipManager.FindChipAt(row, col);
            
                if (!chipBelow)
                {
                    lowestEmpty = row;
                }
                else
                {
                    break;
                }
            }
        
            return lowestEmpty;
        }
        
        private IEnumerator MoveChipToTile(ChipBase chip, int targetRow, int targetCol)
        {
            if (!chip || chip.Tile == null)
            {
                yield break;
            }
        
            var targetTile = BoardSystem.GetTileAt(targetRow, targetCol);
            if (targetTile == null)
            {
                yield break;
            }
        
            chip.Release();
            BoardSystem.RemoveOccupant(chip);
            
            chip.MoveTo(targetTile.Position, 0.2f);
        
            chip.Occupy(targetTile);
            BoardSystem.AddOccupant(chip);
            
            yield return new WaitForSeconds(0.2f);
        }

        private IEnumerator SpawnNewChips()
        {
            var columnCount = BoardSystem.ColumnCount;
            var rowCount = BoardSystem.RowCount;
            
            // Fill from left to right (col: 0 -> N)
            for (var col = 0; col < columnCount; col++)
            {
                // Fill from bottom to top (row: N -> 0)
                for (var row = rowCount - 1; row >= 0; row--)
                {
                    var chipAtPosition = ChipManager.FindChipAt(row, col);

                    if (chipAtPosition)
                    {
                        continue;
                    }
                    var tile = BoardSystem.GetTileAt(row, col);

                    if (tile == null)
                    {
                        continue;
                    }
                    ChipManager.SpawnRandomChipAt(tile);
                        
                    // Small delay for visual effect
                    yield return new WaitForSeconds(0.05f);
                }
            }
        
            yield return new WaitForSeconds(0.1f);
        }
    }
}
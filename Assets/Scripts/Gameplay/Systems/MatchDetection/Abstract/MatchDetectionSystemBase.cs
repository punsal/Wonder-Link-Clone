using Core.Board.Abstract;
using Gameplay.Chip.Abstract;

namespace Gameplay.Systems.MatchDetection.Abstract
{
    /// <summary>
    /// Serves as the base class for implementing match detection systems in a board game.
    /// Provides core logic and structure for detecting potential matches and verifying possible moves.
    /// </summary>
    public abstract class MatchDetectionSystemBase
    {
        protected readonly ChipManagerBase ChipManager;
        private readonly BoardSystemBase _boardSystem;

        protected MatchDetectionSystemBase(BoardSystemBase boardSystem, ChipManagerBase chipManager)
        {
            _boardSystem = boardSystem;
            ChipManager = chipManager;
        }
        
        public bool HasPossibleMoves()
        {
            var rowCount = _boardSystem.RowCount;
            var columnCount = _boardSystem.ColumnCount;

            for (var row = 0; row < rowCount; row++)
            {
                for (var col = 0; col < columnCount; col++)
                {
                    var chip = ChipManager.FindChipAt(row, col);
                    if (chip == null)
                    {
                        continue;
                    }

                    if (CanFormMatch(chip))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        
        protected abstract bool CanFormMatch(ChipBase chip);
        
    }
}
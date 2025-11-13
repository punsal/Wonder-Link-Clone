using System.Collections.Generic;
using Core.Board.Abstract;
using Core.Link.Type;
using Gameplay.Chip.Abstract;
using Gameplay.Systems.MatchDetection.Abstract;

namespace Gameplay.Systems.MatchDetection
{
    /// <summary>
    /// Detects potential matches and valid moves on the board.
    /// </summary>
    public class MatchDetectionSystem : MatchDetectionSystemBase
    {
        public MatchDetectionSystem(BoardSystemBase boardSystem, ChipManagerBase chipManager) : base(boardSystem, chipManager)
        {
            // empty
        }
        
        protected override bool CanFormMatch(ChipBase startChip)
        {
            var linkType = startChip.LinkType;
            var visited = new HashSet<ChipBase>();
            var connected = new List<ChipBase>();

            FindConnectedChips(startChip, linkType, visited, connected);

            // Match requires 3+ chips
            return connected.Count >= 3;
        }
        
        private void FindConnectedChips(ChipBase current, LinkType targetType, HashSet<ChipBase> visited, List<ChipBase> connected)
        {
            if (current == null || visited.Contains(current))
            {
                return;
            }

            if (!current.IsTypeMatch(targetType))
            {
                return;
            }

            visited.Add(current);
            connected.Add(current);

            // Check all 4 adjacent positions (up, down, left, right)
            var tile = current.Tile;
            if (tile == null)
            {
                return;
            }

            var directions = new[]
            {
                new[] { -1, 0 }, // Up
                new[] { 1, 0 },  // Down
                new[] { 0, -1 }, // Left
                new[] { 0, 1 }   // Right
            };

            foreach (var dir in directions)
            {
                var newRow = tile.Row + dir[0];
                var newCol = tile.Column + dir[1];

                var adjacentChip = ChipManager.FindChipAt(newRow, newCol);
                if (adjacentChip != null)
                {
                    FindConnectedChips(adjacentChip, targetType, visited, connected);
                }
            }
        }
    }
}
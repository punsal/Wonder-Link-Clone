using Gameplay.Link.Interface;
using Gameplay.Link.Type;
using UnityEngine;

namespace Gameplay
{
    /// <summary>
    /// Represents a chip that can be linked and interacts with the game board.
    /// Inherits from <see cref="LinkableBase"/> to provide behavior for linking
    /// and adjacency checks specific to a chip.
    /// </summary>
    public class Chip : LinkableBase
    { 
        protected override void Move(Vector3 position)
        {
            transform.position = position;
        }

        protected override void OnLinked()
        {
            // visual feedback
            transform.localScale *= 1.2f;
        }

        protected override void OnUnlinked()
        {
            // visual feedback
            transform.localScale = Vector3.one;
        }
        
        public override bool IsTypeMatch(LinkType type)
        {
            return type == LinkType;
        }

        public override bool IsAdjacent(LinkableBase other)
        {
            if (Tile == null)
            {
                Debug.LogWarning("Tile is null");
                return false;
            }
        
            if (other == null)
            {
                Debug.LogWarning("Other is null");
                return false;
            }

            if (other.Tile == null)
            {
                Debug.LogWarning("Other tile is null");
                return false;
            }
        
            var rowDiff = Mathf.Abs(Tile.Row - other.Tile.Row);
            var columnDiff = Mathf.Abs(Tile.Column - other.Tile.Column);
        
            // force horizontal or vertical
            return (rowDiff == 1 && columnDiff == 0) || (rowDiff == 0 && columnDiff == 1);
        }
    }
}

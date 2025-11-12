using Core.Board.Tile.Abstract;
using UnityEngine;

namespace Core.Board.Tile
{
    /// <summary>
    /// Represents a tile in the board system, handling its visual state and interactions.
    /// </summary>
    /// <remarks>
    /// Inherits from <see cref="TileBase"/> and defines specific behavior for visual representation,
    /// including highlighting and concealing the tile.
    /// </remarks>
    public class BoardTile : TileBase
    {
        [Header("Visuals")]
        [SerializeField] private SpriteRenderer visual;
        [Header("VFX")]
        [SerializeField] private Color highlightColor = Color.green;

        public override Vector3 Position => transform.position;

        private bool _isAwaken;
        
        protected override void OnAwake()
        {
            if (visual == null)
            {
                _isAwaken = false;
                Debug.LogError("Visual is null");
                return;
            }
            
            _isAwaken = true;
        }

        public override void Highlight()
        {
            if (!_isAwaken)
            {
                Debug.LogError("Visual is not awaken");
                return;
            }
            visual.color = highlightColor;
        }

        public override void Conceal()
        {
            if (!_isAwaken)
            {
                Debug.LogError("Visual is not awaken");
                return;
            }
            visual.color = Color.white;
        }
    }
}
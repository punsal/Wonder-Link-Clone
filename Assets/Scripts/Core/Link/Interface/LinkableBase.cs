using Core.Board.Abstract;
using Core.Board.Interface;
using Core.Link.Type;
using UnityEngine;

namespace Core.Link.Interface
{
    /// <summary>
    /// Represents a base class for objects that can be linked and interact with tiles on a game board.
    /// </summary>
    public abstract class LinkableBase : MonoBehaviour, ITileOccupant
    {
        [Header("Linking")]
        [SerializeField] private LinkType linkType;
        public LinkType LinkType => linkType;
        public TileBase Tile { get; private set; }

        private bool _isLinked;

        public void Occupy(TileBase tile)
        {
            if (Tile != null)
            {
                Debug.LogError("Chip already has a tile");
                return;
            }
        
            Tile = tile;
            Move(tile.Position);
        }
        
        public void Release()
        {
            Tile = null;
        }

        protected abstract void Move(Vector3 position);
        
        public void Link()
        {
            if (_isLinked)
            {
                return;
            }

            _isLinked = true;
            // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
            Tile?.Highlight();
            
            OnLinked();
            
            // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
            Debug.Log($"Linked chip {name} at [{Tile?.Row:00}, {Tile?.Column:00}]");
        }

        protected abstract void OnLinked();

        public void Unlink()
        {
            if (!_isLinked)
            {
                return;
            }
            
            _isLinked = false;
            // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
            Tile?.Conceal();
            
            OnUnlinked();
            
            // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
            Debug.Log($"Unlinked chip {name} at [{Tile?.Row:00}, {Tile?.Column:00}]");
        }
        
        protected abstract void OnUnlinked();
        public abstract bool IsTypeMatch(LinkType type);
        public abstract bool IsAdjacent(LinkableBase other);
    }
}
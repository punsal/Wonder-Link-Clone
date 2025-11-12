using Core.Board.Tile.Interface;
using Core.Link.Interface;
using Core.Link.Type;
using UnityEngine;

namespace Core.Link.Abstract
{
    /// <summary>
    /// Represents a base class for objects that can be linked and interact with tiles on a game board.
    /// </summary>
    public abstract class LinkableBase : MonoBehaviour, ILinkable
    {
        [Header("Linking")]
        [SerializeField] private LinkType linkType;
        public LinkType LinkType => linkType;
        public ITile Tile { get; private set; }

        private bool _isLinked;

        public void Occupy(ITile tile)
        {
            if (Tile != null)
            {
                Debug.LogError("Chip already has a tile");
                return;
            }
        
            Tile = tile;
            
            OnOccupied(tile);
        }

        protected virtual void OnOccupied(ITile tile)
        {
            Debug.Log($"{name} occupied the tile {tile.Name}");
        }
        
        public void Release()
        {
            PreRelease(Tile);
            Tile = null;
        }

        protected virtual void PreRelease(ITile tile)
        {
            var tileName = tile == null ? "null" : tile.Name;
            Debug.Log($"{name} released the tile {tileName}");
        }
        
        public void Link()
        {
            if (_isLinked)
            {
                return;
            }

            _isLinked = true;
            Tile?.Highlight();
            
            OnLinked();
            
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
            Tile?.Conceal();
            
            OnUnlinked();
            
            Debug.Log($"Unlinked chip {name} at [{Tile?.Row:00}, {Tile?.Column:00}]");
        }
        
        protected abstract void OnUnlinked();
        public abstract bool IsTypeMatch(LinkType type);
        public abstract bool IsAdjacent(ILinkable other);
    }
}
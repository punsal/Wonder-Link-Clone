using Core.Board.Tile.Interface;
using UnityEngine;

namespace Core.Board.Tile.Abstract
{
    public abstract class TileBase : MonoBehaviour, ITile
    {
        public string Name => _isDestroyed ? string.Empty : name;
        public int Row { get; private set; }
        public int Column { get; private set; }

        private bool _isDestroyed;
        public abstract Vector3 Position { get; }

        private void Awake()
        {
            _isDestroyed = false;
            OnAwake();
        }
        
        protected abstract void OnAwake();

        private void OnDestroy()
        {
            _isDestroyed = true;
        }

        public void Initialize(int row, int column)
        {
            Row = row;
            Column = column;
        }

        public void Destroy()
        {
            if (!_isDestroyed)
            {
                Destroy(gameObject);
            }
        }
    
        public abstract void Highlight();
        public abstract void Conceal();
    }
}
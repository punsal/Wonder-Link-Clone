using System;
using UnityEngine;

namespace Board.Abstract
{
    public abstract class TileBase : MonoBehaviour, IDisposable
    {
        public int Row { get; set; }
        public int Column { get; set; }

        private bool _isDestroyed;
        public Vector3 Position => transform.position;

        private void Awake()
        {
            _isDestroyed = false;
        }

        private void OnDestroy()
        {
            _isDestroyed = true;
        }

        public void Initialize(int row, int column)
        {
            Row = row;
            Column = column;
        }

        public void Dispose()
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
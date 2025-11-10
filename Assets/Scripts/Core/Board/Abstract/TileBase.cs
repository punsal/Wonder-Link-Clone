using System;
using UnityEngine;

namespace Core.Board.Abstract
{
    /// <summary>
    /// Represents the base behavior and properties of a tile within a board system.
    /// </summary>
    /// <remarks>
    /// This abstract class serves as a foundation for building various tile types.
    /// It includes properties for tile position and methods to manage tile state.
    /// Implementers of this class are required to define behavior for highlighting and concealing the tile.
    /// </remarks>
    public abstract class TileBase : MonoBehaviour, IDisposable
    {
        public int Row { get; private set; }
        public int Column { get; private set; }

        private bool _isDestroyed;
        public Vector3 Position => transform.position;

        private void Awake()
        {
            _isDestroyed = false;
        }

        // ReSharper disable once UnusedMember.Global
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
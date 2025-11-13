using System;
using System.Collections.Generic;
using Core.Camera.Provider.Interface;
using Core.Link.Interface;
using UnityEngine;

namespace Core.Link.Abstract
{
    /// <summary>
    /// Abstract base class for managing linking mechanisms within a gameplay system.
    /// Provides the foundation for handling camera interactions, input management,
    /// drag operations, and integration with linkable game elements.
    /// </summary>
    public abstract class LinkSystemBase : IDisposable
    {
        protected UnityEngine.Camera Camera { get; private set; }
        protected LayerMask LayerMask { get; private set; }
        public abstract event Action<List<ILinkable>> OnLinkCompleted;
        public bool IsDragging { get; protected set; }

        protected LinkSystemBase(ICameraProvider cameraProvider, LayerMask layerMask)
        {
            Camera = cameraProvider.Instance;
            LayerMask = layerMask;
        }
    
        public abstract void Dispose();
        public abstract void StartDrag();
        public abstract void UpdateDrag();
        public abstract void EndDrag();
    }
}
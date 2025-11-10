using System;
using System.Collections.Generic;
using Core.Camera.Provider.Interface;
using Gameplay.Link.Abstract;
using Gameplay.Link.Interface;
using Gameplay.Link.Type;
using UnityEngine;

namespace Gameplay.Link
{
    /// <summary>
    /// Represents a system that manages user interactions with linkable objects in a gameplay environment,
    /// providing mechanisms for starting, updating, and concluding drag operations.
    /// </summary>
    public class LinkSystem : LinkSystemBase
    {
        private event Action<List<LinkableBase>> onLinkCompleted;
        public override event Action<List<LinkableBase>> OnInputCompleted
        {
            add => onLinkCompleted += value;
            remove => onLinkCompleted -= value;
        }
    
        private readonly List<LinkableBase> _linkables = new();
        private LinkableBase _lastLinkableBase;
        private LinkType _linkType;

        public LinkSystem(ICameraProvider cameraProvider, LayerMask layerMask) : base(cameraProvider, layerMask)
        {
            IsDragging = false;
        }
    
        public override void Dispose()
        {
            onLinkCompleted = null;
        }

        public override void StartDrag()
        {
            var linkable = GetLinkableAtInputPosition();

            if (linkable == null)
            {
                Debug.LogWarning("No linkable found, won't start drag");
                return;
            }
        
            _linkType = linkable.LinkType;
            IsDragging = true;
            AddLinkable(linkable);
        }

        public override void UpdateDrag()
        {
            var linkable = GetLinkableAtInputPosition();
            if (linkable == null)
            {
                // assume player will find a linkable
                return;
            }

            if (linkable == _lastLinkableBase)
            {
                // still waiting, ignore
                return;
            }

            // did player go back to previous linkable? (undo logic/deselect last)
            var linkedCount = _linkables.Count;
            if (linkedCount >= 2 && linkable == _linkables[linkedCount - 2])
            {
                RemoveLastLinkable();
                return;
            }
        
            // check if linkable is already linked
            if (_linkables.Contains(linkable))
            {
                Debug.LogWarning("Linkable already linked");
                return;
            }
        
            // check if linkable is type match
            if (!linkable.IsTypeMatch(_linkType))
            {
                Debug.LogWarning("Linkable type mismatch");
                return;
            }
        
            // check if linkable is adjacent to last linkable
            if (IsLinkable(linkable))
            {
                AddLinkable(linkable);
            }
        }

        public override void EndDrag()
        {
            IsDragging = false;
            
            // check match-3
            if (_linkables.Count >= 3)
            {
                onLinkCompleted?.Invoke(_linkables);
            }
        
            ClearLinkables();
        }

        private void AddLinkable(LinkableBase linkableBase)
        {
            _linkables.Add(linkableBase);
            _lastLinkableBase = linkableBase;
            linkableBase.Link();
        
            Debug.Log($"Added linkable {linkableBase.GetType().Name}");
        }

        private void RemoveLastLinkable()
        {
            var linkedCount = _linkables.Count;
            if (linkedCount == 0)
            {
                Debug.LogWarning("No linkables to remove");
                return;
            }
        
            var lastLinkable = _linkables[linkedCount - 1];
            _linkables.RemoveAt(linkedCount - 1);
            lastLinkable.Unlink();
        
            _lastLinkableBase = _linkables.Count > 0 ? _linkables[^1] : null;
        
            Debug.Log($"Removed linkable {lastLinkable.GetType().Name}");
        }

        private void ClearLinkables()
        {
            foreach (var linkable in _linkables)
            {
                linkable.Unlink();
            }
        
            _linkables.Clear();
            _lastLinkableBase = null;
        }

        private LinkableBase GetLinkableAtInputPosition()
        {
            var position = Camera.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit2D = Physics2D.Raycast(position, Vector2.zero, Mathf.Infinity, LayerMask);

            if (hit2D.collider != null)
            {
                return hit2D.collider.GetComponent<LinkableBase>();
            }
        
            Debug.LogWarning("No hit");
            return null;
        }

        private bool IsLinkable(LinkableBase linkableBase)
        {
            if (linkableBase == null)
            {
                Debug.LogWarning("Linkable cannot be null");
                return false;
            }

            if (_lastLinkableBase != null)
            {
                return _lastLinkableBase.IsAdjacent(linkableBase);
            }
        
            Debug.LogWarning("No last linkable");
            return false;
        }
    }
}
using System;
using System.Collections.Generic;
using Core.Camera.Provider.Interface;
using Core.Link.Abstract;
using Core.Link.Interface;
using Core.Link.Type;
using UnityEngine;

namespace Core.Link
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
            // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
            var linkable = GetLinkableAtInputPosition();

            if (!linkable)
            {
                // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
                Debug.LogWarning("No linkable found, won't start drag");
                return;
            }

            _linkType = linkable.LinkType;
            IsDragging = true;
            // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
            AddLinkable(linkable);
        }

        public override void UpdateDrag()
        {
            // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
            var linkable = GetLinkableAtInputPosition();
            if (!linkable)
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
                // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
                RemoveLastLinkable();
                return;
            }

            // check if linkable is already linked
            if (_linkables.Contains(linkable))
            {
                // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
                Debug.LogWarning("Linkable already linked");
                return;
            }

            // check if linkable is type match
            if (!linkable.IsTypeMatch(_linkType))
            {
                // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
                Debug.LogWarning("Linkable type mismatch");
                return;
            }

            // check if linkable is adjacent to last linkable
            // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
            if (IsLinkable(linkable))
            {
                // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
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

            // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
            ClearLinkables();
        }

        private void AddLinkable(LinkableBase linkableBase)
        {
            _linkables.Add(linkableBase);
            _lastLinkableBase = linkableBase;
            // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
            linkableBase.Link();

            // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
            Debug.Log($"Added linkable {linkableBase.GetType().Name}");
        }

        private void RemoveLastLinkable()
        {
            var linkedCount = _linkables.Count;
            if (linkedCount == 0)
            {
                // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
                Debug.LogWarning("No linkables to remove");
                return;
            }

            var lastLinkable = _linkables[linkedCount - 1];
            _linkables.RemoveAt(linkedCount - 1);
            // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
            lastLinkable.Unlink();

            _lastLinkableBase = _linkables.Count > 0 ? _linkables[^1] : null;

            // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
            Debug.Log($"Removed linkable {lastLinkable.GetType().Name}");
        }

        private void ClearLinkables()
        {
            foreach (var linkable in _linkables)
            {
                // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
                linkable.Unlink();
            }

            _linkables.Clear();
            _lastLinkableBase = null;
        }

        private LinkableBase GetLinkableAtInputPosition()
        {
            var position = Camera.ScreenToWorldPoint(Input.mousePosition);
            var hit2D = Physics2D.Raycast(position, Vector2.zero, Mathf.Infinity, LayerMask);

            if (hit2D.collider)
            {
                // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
                return hit2D.collider.GetComponent<LinkableBase>();
            }

            // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
            Debug.LogWarning("No hit");
            return null;
        }

        private bool IsLinkable(LinkableBase linkableBase)
        {
            if (!linkableBase)
            {
                // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
                Debug.LogWarning("Linkable cannot be null");
                return false;
            }

            if (_lastLinkableBase)
            {
                // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
                return _lastLinkableBase.IsAdjacent(linkableBase);
            }

            // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
            Debug.LogWarning("No last linkable");
            return false;
        }
    }
}
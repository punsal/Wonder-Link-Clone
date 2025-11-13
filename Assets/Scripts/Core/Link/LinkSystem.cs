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
        private event Action<List<ILinkable>> onLinkCompleted;

        public override event Action<List<ILinkable>> OnLinkCompleted
        {
            add => onLinkCompleted += value;
            remove => onLinkCompleted -= value;
        }

        private readonly List<ILinkable> _linkables = new();
        private ILinkable _lastLinkable;
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

            if (linkable == _lastLinkable)
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

        private void AddLinkable(ILinkable linkable)
        {
            _linkables.Add(linkable);
            _lastLinkable = linkable;
            linkable.Link();

            Debug.Log($"Added linkable {linkable.GetType().Name}");
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

            _lastLinkable = _linkables.Count > 0 ? _linkables[^1] : null;

            Debug.Log($"Removed linkable {lastLinkable.GetType().Name}");
        }

        private void ClearLinkables()
        {
            foreach (var linkable in _linkables)
            {
                linkable.Unlink();
            }

            _linkables.Clear();
            _lastLinkable = null;
        }

        private ILinkable GetLinkableAtInputPosition()
        {
            var position = Camera.ScreenToWorldPoint(Input.mousePosition);
            var hit2D = Physics2D.Raycast(position, Vector2.zero, Mathf.Infinity, LayerMask);

            return hit2D.collider 
                ? hit2D.collider.GetComponent<ILinkable>() 
                : null;
        }

        private bool IsLinkable(ILinkable linkable)
        {
            if (linkable == null)
            {
                Debug.LogWarning("Linkable cannot be null");
                return false;
            }

            if (_lastLinkable != null)
            {
                return _lastLinkable.IsAdjacent(linkable);
            }

            Debug.LogWarning("No last linkable");
            return false;
        }
    }
}
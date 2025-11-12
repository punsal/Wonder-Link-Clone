using System;
using Core.Link.Abstract;
using Gameplay.Input.Abstract;
using UnityEngine;

namespace Gameplay.Input
{
    /// <summary>
    /// Handles touch input for interacting with the gameplay system.
    /// Implements drag functionality such as starting, updating, and ending
    /// drag events in response to touch gestures.
    /// </summary>
    public class TouchInputHandler : InputHandlerBase
    {
        public TouchInputHandler(LinkSystemBase linkSystem) : base(linkSystem)
        {
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            
            // If input is disabled while dragging, end the drag
            if (LinkSystem.IsDragging)
            {
                LinkSystem.EndDrag();
            }
        }

        protected override void ProcessInput()
        {
            if (UnityEngine.Input.touchCount == 0)
            {
                return;
            }

            // single finger
            var touch = UnityEngine.Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    LinkSystem.StartDrag();
                    break;

                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    if (LinkSystem.IsDragging)
                    {
                        LinkSystem.UpdateDrag();
                    }
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    if (LinkSystem.IsDragging)
                    {
                        LinkSystem.EndDrag();
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Platform input-state is not supported");
            }
        }
    }
}
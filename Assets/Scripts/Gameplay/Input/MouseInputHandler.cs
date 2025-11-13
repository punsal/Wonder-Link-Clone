using Core.Link.Abstract;
using Gameplay.Input.Abstract;

namespace Gameplay.Input
{
    /// <summary>
    /// Handles mouse input for interaction within the gameplay system, specifically for initiating,
    /// updating, and ending drag operations. Integrates with the LinkSystemBase to manage the
    /// drag state and ensure proper interaction handling, responding to mouse button states.
    /// </summary>
    public class MouseInputHandler : InputHandlerBase
    {
        public MouseInputHandler(LinkSystemBase linkSystem) : base(linkSystem)
        {
            // empty
        } 
        
        protected override void OnDisabled()
        {
            base.OnDisabled();
            
            // caution to force end drag
            if (LinkSystem.IsDragging)
            {
                LinkSystem.EndDrag();
            }
        }

        protected override void ProcessInput()
        {
            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
                LinkSystem.StartDrag();
            }
            else if (UnityEngine.Input.GetMouseButton(0) && LinkSystem.IsDragging)
            {
                // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
                LinkSystem.UpdateDrag();
            }
            else if (UnityEngine.Input.GetMouseButtonUp(0) && LinkSystem.IsDragging)
            {
                // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
                LinkSystem.EndDrag();
            }
        }
    }
}
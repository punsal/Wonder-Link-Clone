using Core.Link.Abstract;
using UnityEngine;

namespace Gameplay.Input.Abstract
{
    /// <summary>
    /// Abstract base class for managing input handling logic in gameplay systems.
    /// Provides a mechanism to enable, disable, and process input based on the
    /// state of the handler and integrates with the LinkSystemBase for input-related
    /// operations such as dragging functionality.
    /// </summary>
    public abstract class InputHandlerBase
    {
        protected readonly LinkSystemBase LinkSystem;
        private bool IsEnabled { get; set; }

        protected InputHandlerBase(LinkSystemBase linkSystem)
        {
            LinkSystem = linkSystem;
            IsEnabled = false;
        }

        public void Enable()
        {
            IsEnabled = true;
            OnEnabled();
        }

        protected virtual void OnEnabled()
        {
            Debug.Log("Input enabled");
        }

        public void Disable()
        {
            IsEnabled = false;
            OnDisabled();
        }

        protected virtual void OnDisabled()
        {
            Debug.Log("Input disabled");
        }

        public void Update()
        {
            if (!IsEnabled)
            {
                return;
            }

            // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
            ProcessInput();
        }
        
        protected abstract void ProcessInput();
    }
}
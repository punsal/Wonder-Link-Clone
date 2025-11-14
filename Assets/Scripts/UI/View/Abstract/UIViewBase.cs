using UI.Extension;
using UI.State;
using UnityEngine;

namespace UI.View.Abstract
{
    /// <summary>
    /// Base class for UI views in the application. Provides foundational functionality
    /// for state management, visibility toggling, and lifecycle operations of a UI view.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class UIViewBase : MonoBehaviour
    {
        public abstract UIState State { get; }
        
        private CanvasGroup _canvasGroup;
        private bool _isAwaken;
        
        protected bool IsAwaken => _isAwaken;
        
        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
            {
                Debug.LogError("CanvasGroup is null");
                return;
            }
            
            _isAwaken = OnAwake();
        }

        protected abstract bool OnAwake();

        private void OnEnable()
        {
            if (!_isAwaken)
            {
                return;
            }
            
            Enable();   
        }

        protected virtual void Enable()
        {
            // empty
        }

        private void OnDisable()
        {
            if (!_isAwaken)
            {
                return;
            }
            
            Disable();
        }

        protected virtual void Disable()
        {
            // empty
        }

        public virtual void Show()
        {
            _canvasGroup.Show();
        }

        public virtual void Hide()
        {
            _canvasGroup.Hide();
        }
    }
}
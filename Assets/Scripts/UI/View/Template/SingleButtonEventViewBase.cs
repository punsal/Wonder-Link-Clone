using Core.Event;
using UI.View.Abstract;
using UnityEngine;
using UnityEngine.UI;

namespace UI.View.Template
{
    /// <summary>
    /// An abstract base class for a UI view that contains a single button
    /// and triggers a game event upon button click. Provides functionality
    /// to manage the button's event listener during the view's enable and disable
    /// lifecycle states.
    /// </summary>
    public abstract class SingleButtonEventViewBase : UIViewBase
    {
        [Header("References")]
        [SerializeField] private Button button;

        [Header("Events")]
        [SerializeField] private GameEvent clickEvent;

        protected override bool OnAwake()
        {
            if (button && clickEvent)
            {
                return true;
            }
            
            Debug.LogError("Button or GameEvent is null");
            return false;
        }

        protected override void Enable()
        {
            base.Enable();
            button.onClick.AddListener(OnButtonClicked);
        }

        protected override void Disable()
        {
            base.Disable();
            button.onClick.RemoveListener(OnButtonClicked);
        }

        private void OnButtonClicked()
        {
            if (!IsAwaken)
            {
                Debug.LogError("UI is not awaken");
                return;
            }

            clickEvent.Raise();
        }
    }
}
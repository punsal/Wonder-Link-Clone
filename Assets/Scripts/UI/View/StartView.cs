using Core.Event;
using UI.State;
using UI.View.Abstract;
using UnityEngine;
using UnityEngine.UI;

namespace UI.View
{
    /// <summary>
    /// Represents the view for the start screen of the application. This view
    /// is responsible for handling user interactions related to starting the game
    /// and raising the appropriate event to trigger the game's start state.
    /// </summary>
    public class StartView : UIViewBase
    {
        [Header("References")]
        [SerializeField] private Button startGameButton;
        
        [Header("Events")]
        [SerializeField] private GameEvent startGameEvent;
        
        public override UIState State => UIState.StartGame;

        protected override bool OnAwake()
        {
            if (!startGameButton)
            {
                Debug.LogError("StartGameButton is null");
                return false;
            }

            if (!startGameEvent)
            {
                Debug.LogError("StartGameEvent is null");
                return false;
            }
            
            return true;
        }

        protected override void Enable()
        {
            base.Enable();
            startGameButton.onClick.AddListener(OnStartGameButtonClicked);
        }

        protected override void Disable()
        {
            base.Disable();
            startGameButton.onClick.RemoveListener(OnStartGameButtonClicked);
        }

        private void OnStartGameButtonClicked()
        {
            if (!IsAwaken)
            {
                Debug.LogError("UI is not awaken");
                return;
            }
            
            startGameEvent.Raise();
        }
    }
}
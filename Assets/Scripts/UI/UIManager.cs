using System.Collections.Generic;
using System.Linq;
using Core.Event;
using UI.State;
using UI.View.Abstract;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// Manages the user interface components and behavior within the application.
    /// </summary>
    /// <remarks>
    /// This class is responsible for handling the initialization, display, and interactions
    /// between various UI elements in the scene. It acts as a central hub for coordinating
    /// UI activities and ensures consistent UI behavior throughout the application.
    /// </remarks>
    public class UIManager : MonoBehaviour
    {
        [Header("UIReferences")]
        [SerializeField] private List<UIViewBase> views;
        
        [Header("Config")]
        [SerializeField] private UIState initialState = UIState.StartGame;
        
        [Header("Events")]
        [SerializeField] private GameEvent startGameEvent;
        [SerializeField] private GameEvent noMoreTurnsEvent;
        [SerializeField] private GameEvent levelCompletedEvent;
        [SerializeField] private GameEvent shuffleFailedEvent;
        [SerializeField] private GameEvent nextGameEvent;

        private bool _isAwaken;
        
        private void Awake()
        {
            _isAwaken = false;
            if (views == null || views.Count == 0)
            {
                Debug.LogError("No views to show");
                return;
            }

            if (!startGameEvent)
            {
                Debug.LogError("StartGameEvent is null");
                return;
            }
            
            if (!noMoreTurnsEvent)
            {
                Debug.LogError("NoMoreTurnsEvent is null");
                return;
            }

            if (!levelCompletedEvent)
            {
                Debug.LogError("LevelCompletedEvent is null");
                return;
            }
            
            if (!shuffleFailedEvent)
            {
                Debug.LogError("ShuffleFailedEvent is null");
                return;
            }

            if (!nextGameEvent)
            {
                Debug.LogError("NextGameEvent is null");
                return;
            }
            
            _isAwaken = true;
        }

        private void OnEnable()
        {
            if (!_isAwaken)
            {
                return;
            }
            
            startGameEvent.OnEventRaised += HandleStartGameEvent;
            noMoreTurnsEvent.OnEventRaised += HandleNoMoreTurnsEvent;
            levelCompletedEvent.OnEventRaised += HandleLevelCompletedEvent;
            shuffleFailedEvent.OnEventRaised += HandleShuffleFailedEvent;
            nextGameEvent.OnEventRaised += HandleNextGameEvent;
        }

        private void OnDisable()
        {
            if (!_isAwaken)
            {
                return;
            }
            
            startGameEvent.OnEventRaised -= HandleStartGameEvent;
            noMoreTurnsEvent.OnEventRaised -= HandleNoMoreTurnsEvent;
            levelCompletedEvent.OnEventRaised -= HandleLevelCompletedEvent;
            shuffleFailedEvent.OnEventRaised -= HandleShuffleFailedEvent;
            nextGameEvent.OnEventRaised -= HandleNextGameEvent;
        }

        private void Start()
        {
            SetState(initialState);
        }

        private void SetState(UIState state)
        {
            foreach (var view in views)
            {
                view.Hide();
            }

            foreach (var view in views.Where(view => view.State == state))
            {
                view.Show();
            }
        }

        private void HandleStartGameEvent()
        {
            SetState(UIState.Gameplay);
        }
        
        private void HandleNoMoreTurnsEvent()
        {
            SetState(UIState.NoMoreTurns);
        }
        
        private void HandleLevelCompletedEvent()
        {
            SetState(UIState.LevelCompleted);
        }
        
        private void HandleShuffleFailedEvent()
        {
            SetState(UIState.ShuffleFailed);
        }
        
        private void HandleNextGameEvent()
        {
            SetState(UIState.StartGame);
        }
    }
}

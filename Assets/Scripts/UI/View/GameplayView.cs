using Gameplay.Systems.Score.Observable;
using Gameplay.Systems.Turn.Observable;
using TMPro;
using UI.State;
using UI.View.Abstract;
using UnityEngine;

namespace UI.View
{
    /// <summary>
    /// Represents the view for the gameplay state of the application's UI.
    /// Responsible for managing the display of the player's score, level score,
    /// and remaining turn count during gameplay. Reacts to observable changes
    /// in related game state variables.
    /// </summary>
    public class GameplayView : UIViewBase
    {
        [Header("References")]
        [SerializeField] private TextMeshProUGUI levelScoreText;
        [SerializeField] private TextMeshProUGUI playerScoreText;
        [SerializeField] private TextMeshProUGUI remainingTurnText;
        
        [Header("Observables")]
        [SerializeField] private GameScore levelScore;
        [SerializeField] private GameScore playerScore;
        [SerializeField] private GameTurn playerTurn;
        
        public override UIState State => UIState.Gameplay;
        
        protected override bool OnAwake()
        {
            if (!levelScoreText)
            {
                Debug.LogError("LevelScoreText is null");
                return false;
            }

            if (!playerScoreText)
            {
                Debug.LogError("PlayerScoreText is null");
                return false;
            }

            if (!remainingTurnText)
            {
                Debug.LogError("RemainingTurnText is null");
                return false;
            }
            
            if (!levelScore)
            {
                Debug.LogError("LevelScore is null");
                return false;
            }

            if (!playerScore)
            {
                Debug.LogError("PlayerScore is null");
                return false;
            }
            
            if (!playerTurn)
            {
                Debug.LogError("PlayerTurn is null");
                return false;
            }
            
            return true;
        }

        protected override void Enable()
        {
            base.Enable();
            levelScore.OnValueChanged += OnLevelScoreValueChanged;
            playerScore.OnValueChanged += OnPlayerScoreValueChanged;
            playerTurn.OnValueChanged += OnPlayerTurnValueChanged;
        }

        protected override void Disable()
        {
            base.Disable();
            levelScore.OnValueChanged -= OnLevelScoreValueChanged;
            playerScore.OnValueChanged -= OnPlayerScoreValueChanged;
            playerTurn.OnValueChanged -= OnPlayerTurnValueChanged;
        }

        private void Start()
        {
            OnLevelScoreValueChanged(levelScore.Value);
            OnPlayerScoreValueChanged(playerScore.Value);
            OnPlayerTurnValueChanged(playerTurn.Value);
        }

        private void OnLevelScoreValueChanged(int newValue)
        {
            levelScoreText.text = newValue.ToString();
        }
        
        private void OnPlayerScoreValueChanged(int newValue)
        {
            playerScoreText.text = newValue.ToString();
        }
        
        private void OnPlayerTurnValueChanged(int newValue)
        {
            remainingTurnText.text = newValue.ToString();
        }
    }
}
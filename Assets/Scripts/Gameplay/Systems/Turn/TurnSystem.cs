using Gameplay.Systems.Turn.Abstract;
using Gameplay.Systems.Turn.Observable;
using UnityEngine;

namespace Gameplay.Systems.Turn
{
    /// <summary>
    /// Represents the main implementation of a turn-based system for managing player turns in the game.
    /// </summary>
    /// <remarks>
    /// The TurnSystem class inherits from TurnSystemBase and provides functionality to track,
    /// update, and manage the number of turns available within a game session. It observes changes
    /// in the player's turn count and adjusts turn availability based on remaining turns.
    /// </remarks>
    public class TurnSystem : TurnSystemBase
    {
        private bool _isTurnAvailable;
        public override bool IsTurnAvailable => _isTurnAvailable;

        public TurnSystem(GameTurn playerTurn, int initialTurnCount) : base(playerTurn)
        {
            PlayerTurn.SetValue(initialTurnCount);
            _isTurnAvailable = false;
        }

        public override void FinishTurn()
        {
            PlayerTurn.SetValue(PlayerTurn.Value - 1);
        }

        protected override void HandleTurnChanged(int value)
        {
            Debug.Log($"Remaining turns: {value}");
            _isTurnAvailable = PlayerTurn.Value > 0;
        }
    }
}
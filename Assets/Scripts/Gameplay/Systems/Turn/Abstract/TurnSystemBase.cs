using System;
using Gameplay.Systems.Turn.Observable;

namespace Gameplay.Systems.Turn.Abstract
{
    /// <summary>
    /// Abstract base class for a turn-based system in the game.
    /// Manages turn-related operations and synchronization through observable events.
    /// </summary>
    /// <remarks>
    /// This class is intended to be inherited by specific implementations
    /// of turn systems. It provides foundational behavior for managing
    /// turn counts and reacting to turn changes.
    /// </remarks>
    public abstract class TurnSystemBase : IDisposable
    {
        protected readonly GameTurn PlayerTurn;
        
        public abstract bool IsTurnAvailable { get; }

        protected TurnSystemBase(GameTurn playerTurn)
        {
            PlayerTurn = playerTurn;
            
            PlayerTurn.OnValueChanged += HandleTurnChanged;
        }
        
        public abstract void FinishTurn();
        
        protected abstract void HandleTurnChanged(int value);

        public void Dispose()
        {
            PlayerTurn.OnValueChanged -= HandleTurnChanged;
        }
    }
}
using System;
using Gameplay.Systems.Score.Amount.Interface;
using Gameplay.Systems.Score.Observable;

namespace Gameplay.Systems.Score.Abstract
{
    /// <summary>
    /// Represents the base class for score management systems in a gameplay context.
    /// It provides functionality to manage player score, track score increments,
    /// and determine if specific score criteria have been met.
    /// </summary>
    public abstract class ScoreSystemBase : IDisposable
    {
        protected readonly GameScore LevelScore;
        private readonly GameScore _playerScore;
        
        public abstract bool IsScoreReached { get; }

        protected ScoreSystemBase(GameScore levelScore, GameScore playerScore)
        {
            LevelScore = levelScore;
            _playerScore = playerScore;
            
            _playerScore.OnValueChanged += HandlePlayerScoreChanged;
        }
        
        public void AddScore(IScoreAmountProvider provider)
        {
            _playerScore.SetValue(_playerScore.Value + provider.Instance);
        }

        protected abstract void HandlePlayerScoreChanged(int value);

        public virtual void Dispose()
        {
            _playerScore.OnValueChanged -= HandlePlayerScoreChanged;
        }
    }
}
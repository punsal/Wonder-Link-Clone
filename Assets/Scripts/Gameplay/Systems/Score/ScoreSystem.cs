using Gameplay.Systems.Score.Abstract;
using Gameplay.Systems.Score.Observable;
using UnityEngine;

namespace Gameplay.Systems.Score
{
    /// <summary>
    /// Represents a specific implementation of a score management system for gameplay.
    /// It tracks the player's score and determines if the target score has been reached
    /// based on the current score and predefined level score criteria.
    /// </summary>
    public class ScoreSystem : ScoreSystemBase
    {
        private bool _isScoreReached;

        public override bool IsScoreReached => _isScoreReached;

        public ScoreSystem(GameScore levelScore, GameScore playerScore) : base(levelScore, playerScore)
        {
            _isScoreReached = false;
        }

        protected override void HandlePlayerScoreChanged(int value)
        {
            Debug.Log($"Player score changed to {value}, needs to reach {LevelScore.Value}");
            _isScoreReached = value >= LevelScore.Value;
        }
    }
}
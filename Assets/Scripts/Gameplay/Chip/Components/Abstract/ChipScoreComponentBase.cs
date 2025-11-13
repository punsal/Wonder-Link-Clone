using Gameplay.Systems.Score.Amount.Interface;
using UnityEngine;

namespace Gameplay.Chip.Components.Abstract
{
    /// <summary>
    /// Provides a base implementation for chip score components in the game.
    /// This abstract class manages score-related functionality for chips
    /// by implementing the <see cref="IScoreAmountProvider"/> interface and exposing
    /// common behavior and properties for derived score component implementations.
    /// </summary>
    public abstract class ChipScoreComponentBase : MonoBehaviour, IScoreAmountProvider
    {
        [SerializeField] private int scoreAmount;
        public int Instance => scoreAmount;
        
        public abstract IScoreAmountProvider ScoreAmountProvider { get; }
    }
}
using Core.Provider.Interface;

namespace Gameplay.Systems.Score.Amount.Interface
{
    /// <summary>
    /// Represents a provider interface responsible for supplying an integer value
    /// that may be used for score calculations or similar purposes within the gameplay systems.
    /// </summary>
    public interface IScoreAmountProvider : IProvider<int>
    {
        
    }
}
using Core.Observable.Abstract;
using UnityEngine;

namespace Gameplay.Systems.Turn.Observable
{
    /// <summary>
    /// Represents a scriptable observable that tracks the game's turn count.
    /// Inherits from <see cref="ScriptableObservableBase{T}"/> with an integer value type.
    /// </summary>
    /// <remarks>
    /// Designed to be utilized as a ScriptableObject in Unity. The value represents the current turn count,
    /// and changes to the value can be observed through the OnValueChanged event.
    /// </remarks>
    [CreateAssetMenu(menuName = "Observables/GameTurn", fileName = "New GameTurn")]
    public class GameTurn : ScriptableObservableBase<int>
    {
        // empty
    }
}
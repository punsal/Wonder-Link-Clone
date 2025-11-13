using Core.Observable.Abstract;
using UnityEngine;

namespace Gameplay.Systems.Score.Observable
{
    /// <summary>
    /// Represents a game score as a ScriptableObject, which is observable and can notify
    /// listeners when the score value changes. This provides a reactive mechanism for score management
    /// in gameplay systems.
    /// </summary>
    /// <remarks>
    /// This class inherits from <see cref="ScriptableObservableBase{T}"/> where T is of type int, allowing
    /// the score to be tracked as an integer value. It enables score changes to be observed and handled
    /// dynamically within gameplay.
    /// </remarks>
    /// <example>
    /// Use this class with Unity's ScriptableObject system to manage and observe score changes
    /// throughout your game, enabling features such as score-based triggers and UI updates.
    /// </example>
    [CreateAssetMenu(menuName = "Observables/GameScore", fileName = "New GameScore")]
    public class GameScore : ScriptableObservableBase<int>
    {
        
    }
}
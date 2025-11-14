using System;
using UnityEngine;

namespace Core.Event
{
    [CreateAssetMenu(fileName = "New GameEvent", menuName = "GameEvent")]
    public class GameEvent : ScriptableObject
    {
        private event Action onEventRaised;
        public event Action OnEventRaised
        {
            add => onEventRaised += value;
            remove => onEventRaised -= value;
        }

        public void Raise()
        {
            Debug.Log($"Event raised: {name}");
            onEventRaised?.Invoke();
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using Core.Board.Abstract;
using Core.Runner.Interface;
using Gameplay.Chip.Abstract;
using Gameplay.Systems.MatchDetection.Abstract;
using UnityEngine;

namespace Gameplay.Systems.BoardRefill.Abstract
{
    /// <summary>
    /// Represents the base class for a board refill system, responsible for refilling the board
    /// with chips and managing the refill process.
    /// </summary>
    public abstract class BoardRefillSystemBase : IDisposable
    {
        protected readonly BoardSystemBase BoardSystem;
        protected readonly ChipManagerBase ChipManager;
        protected readonly ICoroutineRunner CoroutineRunner;

        protected abstract MatchDetectionSystemBase MatchDetectionSystem { get; }
        
        private event Action onRefillCompleted;
        public event Action OnRefillCompleted
        {
            add => onRefillCompleted += value;
            remove => onRefillCompleted -= value;
        }

        protected BoardRefillSystemBase(
            BoardSystemBase boardSystem, 
            ChipManagerBase chipManager, 
            ICoroutineRunner coroutineRunner)
        {
            BoardSystem = boardSystem;
            ChipManager = chipManager;
            CoroutineRunner = coroutineRunner;
        }

        public void StartRefill(List<ChipBase> chips)
        {
            CoroutineRunner.StartCoroutine(WaitForRefillComplete(chips));
        }

        private IEnumerator WaitForRefillComplete(List<ChipBase> chips)
        {
            yield return CoroutineRunner.StartCoroutine(Refill(chips));
            
            Debug.Log("Refill completed.");
            onRefillCompleted?.Invoke();
        }

        protected abstract IEnumerator Refill(List<ChipBase> chips);
        
        public void Dispose()
        {
            onRefillCompleted = null;
        }
    }
}
using System;
using System.Collections;
using Core.Board.Abstract;
using Core.Runner.Interface;
using Gameplay.Chip.Abstract;
using UnityEngine;

namespace Gameplay.Systems.Shuffle.Abstract
{
    /// <summary>
    /// Base class for shuffling chips on the board.
    /// </summary>
    public abstract class ShuffleSystemBase : IDisposable
    {
        protected readonly BoardSystemBase BoardSystem;
        protected readonly ChipManagerBase ChipManager;
        protected readonly ICoroutineRunner CoroutineRunner;
        
        private int _shuffleCount;
        public int ShuffleCount => _shuffleCount;

        private event Action onShuffleCompleted;
        public event Action OnShuffleCompleted
        {
            add => onShuffleCompleted += value;
            remove => onShuffleCompleted -= value;
        }
        
        protected ShuffleSystemBase(
            BoardSystemBase boardSystem, 
            ChipManagerBase chipManager, 
            ICoroutineRunner coroutineRunner)
        {
            BoardSystem = boardSystem;
            ChipManager = chipManager;
            CoroutineRunner = coroutineRunner;
            
            _shuffleCount = 10;
        }

        public void StartShuffle()
        {
            CoroutineRunner.StartCoroutine(WaitForShuffle());
        }

        private IEnumerator WaitForShuffle()
        {
            Debug.Log("Shuffling board...");
            yield return CoroutineRunner.StartCoroutine(Shuffle());
            Debug.Log("Shuffle complete");
            _shuffleCount++;
            onShuffleCompleted?.Invoke();
        }

        protected abstract IEnumerator Shuffle();

        public void Dispose()
        {
            onShuffleCompleted = null;
        }
    }
}
using Core.Link.Abstract;
using Gameplay.Chip.Components.Abstract;
using Gameplay.Systems.Score.Amount.Interface;
using UnityEngine;

namespace Gameplay.Chip.Abstract
{
    /// <summary>
    /// Represents the base class for a chip in the game, providing core functionality
    /// for movement and interaction with tiles on the game board. Inherits from
    /// <see cref="LinkableBase"/> to include linking behavior.
    /// </summary>
    public abstract class ChipBase : LinkableBase
    {
        [Header("Components")]
        [SerializeField] private ChipAnimatorComponentBase animator;
        [SerializeField] private ChipScoreComponentBase score;
        
        public IScoreAmountProvider Score => score;
        
        private void Awake()
        {
            OnAwake();
        }

        protected virtual void OnAwake()
        {
            if (animator == null)
            {
                Debug.LogError("Animator component not assigned");
            }
        }

        protected override void OnLinked()
        {
            animator?.PlayLinkEffect();
        }

        protected override void OnUnlinked()
        {
            animator?.PlayUnlinkEffect();
        }

        public void Destroy()
        {
            animator?.AnimateDestruction();
        }
        
        public void MoveTo(Vector3 position, float duration)
        {
            animator.AnimateMovement(position, duration);
        }
    }
}
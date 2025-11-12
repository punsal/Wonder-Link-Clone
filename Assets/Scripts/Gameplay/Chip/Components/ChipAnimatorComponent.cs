using System.Collections;
using Gameplay.Chip.Components.Abstract;
using UnityEngine;

namespace Gameplay.Chip.Components
{
    /// <summary>
    /// Handles animations related to chip behavior, including linking, unlinking,
    /// destruction, and movement. This class extends from ChipAnimatorComponentBase
    /// and provides concrete implementations for animation effects.
    /// </summary>
    public class ChipAnimatorComponent : ChipAnimatorComponentBase
    {
        [Header("Settings")]
        [SerializeField] private float linkScaleMultiplier = 1.2f;
        [SerializeField] private float destroyDuration = 0.2f;
        [SerializeField] private float moveDuration = 0.2f;
        
        private Vector3 _originalScale;
        private Coroutine _currentAnimation;

        private void Awake()
        {
            _originalScale = transform.localScale;
        }

        public override void PlayLinkEffect()
        {
            StopCurrentAnimation();
            transform.localScale = _originalScale * linkScaleMultiplier;
        }

        public override void PlayUnlinkEffect()
        {
            StopCurrentAnimation();
            transform.localScale = _originalScale;
        }

        public override void AnimateDestruction()
        {
            StopCurrentAnimation();
            _currentAnimation = StartCoroutine(ScaleDownAnimation(destroyDuration));
        }

        public override void AnimateMovement(Vector3 target, float duration)
        {
            StopCurrentAnimation();
            _currentAnimation = StartCoroutine(MoveAnimation(target, moveDuration));
        }

        private IEnumerator ScaleDownAnimation(float duration)
        {
            var startScale = transform.localScale;
            var elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var t = elapsed / duration;
                transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
                
                yield return null;
            }
        }

        private IEnumerator MoveAnimation(Vector3 target, float duration)
        {
            var startPosition = transform.position;
            var elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var t = elapsed / duration;
                transform.position = Vector3.Lerp(startPosition, target, t);
                
                yield return null;
            }
        }

        private void StopCurrentAnimation()
        {
            if (_currentAnimation == null)
            {
                return;
            }
            StopCoroutine(_currentAnimation);
            _currentAnimation = null;
        }
    }
}
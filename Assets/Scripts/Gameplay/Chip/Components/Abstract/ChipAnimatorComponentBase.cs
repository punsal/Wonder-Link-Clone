using UnityEngine;

namespace Gameplay.Chip.Components.Abstract
{
    /// <summary>
    /// Provides a base class for handling animations related to chip components in the game.
    /// This abstract class defines core animation functionalities such as linking, unlinking,
    /// destruction, and movement, to be implemented by derived classes.
    /// </summary>
    public abstract class ChipAnimatorComponentBase : MonoBehaviour
    {
        public abstract void PlayLinkEffect();
        public abstract void PlayUnlinkEffect();
        public abstract void AnimateDestruction();
        public abstract void AnimateMovement(Vector3 target, float duration);
    }
}
using UI.State;
using UI.View.Template;

namespace UI.View
{
    /// <summary>
    /// Represents the view displayed when a shuffle operation fails in the game.
    /// Inherits from <see cref="SingleButtonEventViewBase"/> to provide a user interface
    /// with a single button that can trigger an associated event upon user interaction.
    /// </summary>
    public class ShuffleFailedView : SingleButtonEventViewBase
    {
        public override UIState State => UIState.ShuffleFailed;
    }
}
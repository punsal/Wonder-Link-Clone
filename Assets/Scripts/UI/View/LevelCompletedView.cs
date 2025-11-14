using UI.State;
using UI.View.Template;

namespace UI.View
{
    /// <summary>
    /// Represents the UI view displayed when a level is completed.
    /// This view extends the base functionality of SingleButtonEventViewBase
    /// to handle interactions specific to the LevelCompleted UI state.
    /// </summary>
    public class LevelCompletedView : SingleButtonEventViewBase
    {
        public override UIState State => UIState.LevelCompleted;
    }
}
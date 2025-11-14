using UI.State;
using UI.View.Template;

namespace UI.View
{
    /// <summary>
    /// Represents a view corresponding to the "No More Turns" state in the UI lifecycle.
    /// Extends the SingleButtonEventViewBase to include a single-button event handling mechanism
    /// specific to the NoMoreTurns state.
    /// The state for this view is defined by the UIState enumeration as UIState.NoMoreTurns.
    /// </summary>
    public class NoMoreTurnsView : SingleButtonEventViewBase
    {
        public override UIState State => UIState.NoMoreTurns;
    }
}
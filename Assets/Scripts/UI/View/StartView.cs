using UI.State;
using UI.View.Template;

namespace UI.View
{
    /// <summary>
    /// Represents the view for the start screen of the application. This view
    /// is responsible for handling user interactions related to starting the game
    /// and raising the appropriate event to trigger the game's start state.
    /// </summary>
    public class StartView : SingleButtonEventViewBase
    {
        public override UIState State => UIState.StartGame;
    }
}
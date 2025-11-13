using Gameplay.Chip.Components.Abstract;
using Gameplay.Systems.Score.Amount.Interface;

namespace Gameplay.Chip.Components
{
    /// <summary>
    /// A concrete implementation of <see cref="ChipScoreComponentBase"/>
    /// that serves as a chip score component in a gameplay system.
    /// </summary>
    /// <remarks>
    /// The <see cref="ChipScoreComponent"/> class provides the score calculation behavior
    /// for game chips. It exposes an <see cref="IScoreAmountProvider"/> instance to integrate
    /// with the scoring system.
    /// </remarks>
    public class ChipScoreComponent : ChipScoreComponentBase
    {
        public override IScoreAmountProvider ScoreAmountProvider => this;
    }
}
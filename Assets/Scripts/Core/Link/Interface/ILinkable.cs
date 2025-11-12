using Core.Board.Interface;
using Core.Link.Type;

namespace Core.Link.Interface
{
    /// <summary>
    /// Represents an object that can participate in a linking system, with specific behaviors
    /// related to linking, unlinking, type matching, and adjacency checks.
    /// </summary>
    public interface ILinkable : ITileOccupant
    {
        LinkType LinkType { get; }
        void Link();
        void Unlink();
        bool IsTypeMatch(LinkType type);
        bool IsAdjacent(ILinkable other);
    }
}
namespace WisdomWarrior.Editor.Core.ShadowTree;

public interface IInspectableObjectTracker
{
    string Name { get; }
    IReadOnlyList<PropertyTracker> Properties { get; }
    bool IsDirty { get; }
}

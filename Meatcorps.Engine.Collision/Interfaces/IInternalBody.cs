namespace Meatcorps.Engine.Collision.Interfaces;

internal interface IInternalBody
{
    void SetStableIndex(int index);

    void MarkBoundsDirty();
}
namespace Arnis.Core
{
    public interface ISink
    {
        string Name { get; }
        string Description { get; }

        void Flush(Workspace workspace);
    }
}

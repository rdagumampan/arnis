namespace Arnis.Core
{
    public interface ISink
    {
        void Flush(Workspace workspace);
    }
}

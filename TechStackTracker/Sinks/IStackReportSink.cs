using System.Threading.Tasks;

namespace TechStackTracker.Sinks
{
    interface IStackReportSink
    {
        void Flush();
    }
}

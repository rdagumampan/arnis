using System.Collections.Generic;

namespace TechStackTracker
{
    public class StackReport
    {
        public List<StackItem> Results { get; set; }
        public List<string> Errors { get; set; }
    }
}
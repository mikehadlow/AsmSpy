using System.Collections.Generic;

namespace AsmSpy.Core
{
    public class VisualizerOptions
    {
        public bool SkipSystem { get; set; }
        public bool OnlyConflicts { get; set; }
        public string ReferencedStartsWith { get; set; }
        public IList<string> Exclude { get; set; }
    }
}

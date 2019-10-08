using System;

namespace AsmSpy.Core
{
    public class VisualizerOptions
    {
        public bool SkipSystem { get; }
        public bool OnlyConflicts { get; }
        public string ReferencedStartsWith { get; }

        public VisualizerOptions(bool skipSystem, bool onlyConflicts, string referencedStartsWith)
        {
            SkipSystem = skipSystem;
            OnlyConflicts = onlyConflicts;
            ReferencedStartsWith = referencedStartsWith ?? throw new ArgumentNullException(nameof(referencedStartsWith));
        }
    }
}

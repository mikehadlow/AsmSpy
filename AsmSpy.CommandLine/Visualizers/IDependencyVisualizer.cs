using AsmSpy.Core;

using Microsoft.Extensions.CommandLineUtils;

namespace AsmSpy.CommandLine.Visualizers
{
    public interface IDependencyVisualizer
    {
        void Visualize(DependencyAnalyzerResult result, ILogger logger, VisualizerOptions visualizerOptions);
        void CreateOption(CommandLineApplication commandLineApplication);
        bool IsConfigured();
    }
}

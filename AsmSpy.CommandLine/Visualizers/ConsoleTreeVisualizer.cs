using AsmSpy.Core;

using Microsoft.Extensions.CommandLineUtils;

using System;
using System.Linq;

using static System.Console;

namespace AsmSpy.CommandLine.Visualizers
{
    public class ConsoleTreeVisualizer : IDependencyVisualizer
    {
        CommandOption noConsole;

        public void CreateOption(CommandLineApplication commandLineApplication)
        {
            noConsole = commandLineApplication.Option("-tr|--tree", "Output a console tree view of dependencies.", CommandOptionType.NoValue);
        }

        public bool IsConfigured() => noConsole.HasValue();

        public void Visualize(DependencyAnalyzerResult result, ILogger logger, VisualizerOptions visualizerOptions)
        {

            foreach(var root in result.Roots)
            {
                WalkDependencies(root);
            }


            void WalkDependencies(IAssemblyReferenceInfo assembly, string tab = "", bool lastParent = true)
            {
                var label = lastParent ? endNodeLabel : nodeLabel;
                var currentForgroundColor = ForegroundColor;
                Write($"{tab}{label}");
                ForegroundColor = SelectConsoleColor(assembly.AssemblySource);

                var alternativeVersion = assembly.AlternativeFoundVersion == null
                    ? ""
                    : $" -> {assembly.AlternativeFoundVersion.AssemblyName.Version.ToString()}";

                WriteLine($"{assembly.AssemblyName.Name} {assembly.AssemblyName.Version.ToString()}{alternativeVersion}");
                ForegroundColor = currentForgroundColor;

                assembly = assembly.AlternativeFoundVersion ?? assembly;

                var count = 1;
                var totalChildren = assembly.References.Count();
                foreach(var dependency in assembly.References)
                {
                    if(dependency.AssemblySource == AssemblySource.GlobalAssemblyCache && visualizerOptions.SkipSystem)
                    {
                        continue;
                    }
                    var parentLast = count++ == totalChildren;
                    var parentLabel = lastParent ? tabLabel : continuationLabel;
                    WalkDependencies(dependency, tab + parentLabel, parentLast);
                }
            }
        }

        private ConsoleColor SelectConsoleColor(AssemblySource assemblySource)
        {
            switch (assemblySource)
            {
                case AssemblySource.NotFound:
                    return AssemblyNotFoundColor;
                case AssemblySource.Local:
                    return AssemblyLocalColor;
                case AssemblySource.GlobalAssemblyCache:
                    return AssemblyGlobalAssemblyCacheColor;
                case AssemblySource.Unknown:
                    return AssemblyUnknownColor;
                default:
                    throw new InvalidOperationException("Unknown AssemblySource value.");
            }
        }

        private const ConsoleColor AssemblyNotFoundColor = ConsoleColor.Red;
        private const ConsoleColor AssemblyLocalColor = ConsoleColor.Green;
        private const ConsoleColor AssemblyGlobalAssemblyCacheColor = ConsoleColor.Yellow;
        private const ConsoleColor AssemblyUnknownColor = ConsoleColor.Magenta;

        private const string tabLabel = "    ";
        private const string nodeLabel = "├──";
        private const string endNodeLabel = "└──";
        private const string continuationLabel = "│  ";
    }
}

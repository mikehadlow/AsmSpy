using AsmSpy.Core;

using Microsoft.Extensions.CommandLineUtils;

using System;
using System.Linq;

using static System.Console;

namespace AsmSpy.CommandLine.Visualizers
{
    public class ConsoleTreeVisualizer : IDependencyVisualizer
    {
        private CommandOption treeOption;
        private CommandOption treeDepthOption;
        private CommandOption treeLabelOption;

        public void CreateOption(CommandLineApplication commandLineApplication)
        {
            treeOption = commandLineApplication.Option("-tr|--tree", "Output a console tree view of dependencies.", CommandOptionType.NoValue);
            treeDepthOption = commandLineApplication.Option("-trd|--treedepth", "Limit tree depth (in compbinaison with --tree).", CommandOptionType.SingleValue);
            treeLabelOption = commandLineApplication.Option("-trl|--treelabel", "Add [Level n] label in tree view of dependencies.", CommandOptionType.NoValue);
        }

        public bool IsConfigured() => treeOption.HasValue();

        private int? CalcMaxDepth()
        {
            if (treeDepthOption.HasValue())
            {
                string optionValue = treeDepthOption.Value();
                if (!int.TryParse(optionValue, out var maxDepthValue))
                {
                    throw new ArgumentException("treedepth must be a int greater or equals to zero. Value: \"" + optionValue + "\"");
                }
                if (maxDepthValue < 0)
                {
                    throw new ArgumentException("treedepth must be a int greater or equals to zero. Value: \"" + optionValue + "\"");
                }
                return maxDepthValue;
            }
            return null;
        }

        private bool IsNodeLabelEnabled() => treeLabelOption.HasValue();

        private void WriteTreeNodeLabel(string tab, bool lastParent, int level)
        {
            var label = lastParent ? endNodeLabel : nodeLabel;

            Write($"{tab}{label}");
            if (IsNodeLabelEnabled())
            {
                Write($"[LEVEL {level}] ");
            }
        }

        public void Visualize(DependencyAnalyzerResult result, ILogger logger, VisualizerOptions visualizerOptions)
        {
            int? maxDepth = CalcMaxDepth();

            foreach (var root in result.Roots)
            {
                WalkDependencies(root);
            }


            void WalkDependencies(IAssemblyReferenceInfo assembly, string tab = "", bool lastParent = true, int level = 0)
            {
                // Break if max depth is reached.
                if (maxDepth.HasValue && level > maxDepth)
                {
                    return;
                }

                var currentForgroundColor = ForegroundColor;
                WriteTreeNodeLabel(tab, lastParent, level);
                ForegroundColor = SelectConsoleColor(assembly.AssemblySource);

                var alternativeVersion = assembly.AlternativeFoundVersion == null
                    ? ""
                    : $" -> {assembly.AlternativeFoundVersion.AssemblyName.Version.ToString()}";

                WriteLine($"{assembly.AssemblyName.Name} {assembly.AssemblyName.Version.ToString()}{alternativeVersion}");
                ForegroundColor = currentForgroundColor;

                assembly = assembly.AlternativeFoundVersion ?? assembly;

                var count = 1;
                var totalChildren = assembly.References.Count();
                int nextLevel = level + 1;
                foreach (var dependency in assembly.References)
                {
                    if (dependency.AssemblySource == AssemblySource.GlobalAssemblyCache && visualizerOptions.SkipSystem)
                    {
                        continue;
                    }
                    var parentLast = count++ == totalChildren;
                    var parentLabel = lastParent ? tabLabel : continuationLabel;
                    WalkDependencies(dependency, tab + parentLabel, parentLast, nextLevel);
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

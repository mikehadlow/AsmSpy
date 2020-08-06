using AsmSpy.Core;

using Microsoft.Extensions.CommandLineUtils;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace AsmSpy.CommandLine.Visualizers
{
    public class ConsoleVisualizer : IDependencyVisualizer
    {
        private const ConsoleColor AssemblyNotFoundColor = ConsoleColor.Red;
        private const ConsoleColor AssemblyLocalColor = ConsoleColor.Green;
        private const ConsoleColor AssemblyAlternativeColor = ConsoleColor.DarkYellow;
        private const ConsoleColor AssemblyLocalRedirectedColor = ConsoleColor.DarkGreen;
        private const ConsoleColor AssemblyGlobalAssemblyCacheColor = ConsoleColor.Yellow;
        private const ConsoleColor AssemblyUnknownColor = ConsoleColor.Magenta;

        public virtual void Visualize(DependencyAnalyzerResult result, ILogger logger, VisualizerOptions visualizerOptions)
        {
            if (result.AnalyzedFiles.Count <= 0)
            {
                Console.WriteLine(AsmSpy_CommandLine.No_assemblies_files_found_in_directory);
                return;
            }

            if (visualizerOptions.OnlyConflicts)
            {
                Console.WriteLine(AsmSpy_CommandLine.Detailing_only_conflicting_assembly_references);
            }

            var assemblyGroups = result.Assemblies.Values.GroupBy(x => x.RedirectedAssemblyName);

            foreach (var assemblyGroup in assemblyGroups.OrderBy(i => i.Key.Name))
            {
                var assemblyInfos = assemblyGroup.OrderBy(x => x.AssemblyName.Name).ToList();
                if (visualizerOptions.OnlyConflicts && assemblyInfos.Count <= 1)
                {
                    if (assemblyInfos.Count == 1 && assemblyInfos[0].AssemblySource == AssemblySource.Local)
                    {
                        continue;
                    }

                    if (assemblyInfos.Count <= 0)
                    {
                        continue;
                    }
                }

                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(AsmSpy_CommandLine.Reference);
                Console.ForegroundColor = GetMainNameColor(assemblyInfos);
                Console.WriteLine(AsmSpy_CommandLine.ConsoleVisualizer_Visualize__0_, assemblyGroup.Key);

                foreach (var assemblyInfo in assemblyInfos)
                {
                    VisualizeAssemblyReferenceInfo(assemblyInfo);
                }

                Console.WriteLine();
            }
            Console.ResetColor();
        }

        protected virtual ConsoleColor GetMainNameColor(IList<AssemblyReferenceInfo> assemblyReferenceInfoList)
        {
            ConsoleColor mainNameColor;
            if (assemblyReferenceInfoList.Any(x => x.AssemblySource == AssemblySource.Unknown))
            {
                mainNameColor = AssemblyUnknownColor;
            }
            else if (assemblyReferenceInfoList.Any(x => x.AssemblySource == AssemblySource.NotFound && x.HasAlternativeVersion))
            {
                mainNameColor = AssemblyAlternativeColor;
            }
            else if (assemblyReferenceInfoList.Any(x => x.AssemblySource == AssemblySource.NotFound))
            {
                mainNameColor = AssemblyNotFoundColor;
            }
            else if (assemblyReferenceInfoList.Any(x => x.AssemblySource == AssemblySource.GlobalAssemblyCache))
            {
                mainNameColor = AssemblyGlobalAssemblyCacheColor;
            }
            else
            {
                if (assemblyReferenceInfoList.All(x => x.AssemblyName.FullName == x.RedirectedAssemblyName.FullName))
                {
                    mainNameColor = AssemblyLocalColor;
                }
                else
                {
                    mainNameColor = AssemblyLocalRedirectedColor;
                }
            }
            return mainNameColor;
        }

        protected virtual void VisualizeAssemblyReferenceInfo(AssemblyReferenceInfo assemblyReferenceInfo)
        {
            if (assemblyReferenceInfo == null)
            {
                throw new ArgumentNullException(nameof(assemblyReferenceInfo));
            }
            ConsoleColor statusColor;
            switch (assemblyReferenceInfo.AssemblySource)
            {
                case AssemblySource.NotFound:
                    statusColor = assemblyReferenceInfo.HasAlternativeVersion ? AssemblyAlternativeColor : AssemblyNotFoundColor;
                    break;
                case AssemblySource.Local:
                    statusColor = AssemblyLocalColor;
                    break;
                case AssemblySource.GlobalAssemblyCache:
                    statusColor = AssemblyGlobalAssemblyCacheColor;
                    break;
                case AssemblySource.Unknown:
                    statusColor = AssemblyUnknownColor;
                    break;
                default:
                    throw new InvalidEnumArgumentException(AsmSpy_CommandLine.Invalid_AssemblySource);
            }
            Console.ForegroundColor = statusColor;
            Console.WriteLine(AsmSpy_CommandLine.ConsoleVisualizer_VisualizeAssemblyReferenceInfo____0_, assemblyReferenceInfo.AlternativeFoundVersion?.AssemblyName ?? assemblyReferenceInfo.AssemblyName);
            Console.Write(AsmSpy_CommandLine.Source_, assemblyReferenceInfo.AssemblySource);

            if (assemblyReferenceInfo.AssemblySource != AssemblySource.NotFound)
            {
                Console.WriteLine(AsmSpy_CommandLine.Location_, assemblyReferenceInfo.ReflectionOnlyAssembly.Location);
            }
            else
            {
                if (assemblyReferenceInfo.HasAlternativeVersion) 
                {
                    Console.Write(AsmSpy_CommandLine.AlternativeVersionFound);
                }
                Console.WriteLine();
            }

            foreach (var referer in assemblyReferenceInfo.ReferencedBy.OrderBy(x => x.AssemblyName.ToString()))
            {
                Console.ForegroundColor = statusColor;
                Console.Write(AsmSpy_CommandLine.ConsoleVisualizer_VisualizeAssemblyReferenceInfo______0_, assemblyReferenceInfo.AssemblyName.Version);

                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(AsmSpy_CommandLine.by);

                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine(AsmSpy_CommandLine.ConsoleVisualizer_VisualizeAssemblyReferenceInfo__0_, referer.AssemblyName);
            }
        }

        CommandOption noConsole;

        public void CreateOption(CommandLineApplication commandLineApplication)
        {
            noConsole = commandLineApplication.Option("-nc|--noconsole", "Do not show references on console.", CommandOptionType.NoValue);
        }

        public bool IsConfigured() => !noConsole.HasValue();
    }
}

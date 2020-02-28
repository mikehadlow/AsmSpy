using AsmSpy.Core;
using Microsoft.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace AsmSpy.CommandLine
{
    public class ConsoleUnreferencedAssembliesVisualizer : IDependencyVisualizer
    {
        private const ConsoleColor AssemblyNotFoundColor = ConsoleColor.Red;
        private const ConsoleColor AssemblyLocalColor = ConsoleColor.Green;
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

            Console.WriteLine("Unreferenced Assemblies From Initial Files");

            IEnumerable<AssemblyReferenceInfo> assemblyFiles = 
                result.Assemblies.Values.Where(x => x.FileName != string.Empty && x.ReferencedBy.Count == 0);

            foreach (var assembly in assemblyFiles.OrderBy(i => i.AssemblyName.ToString()))
            {
                Console.ForegroundColor = GetMainNameColor(assembly);

                VisualizeAssemblyReferenceInfo(assembly);

                Console.WriteLine();
            }

            Console.ResetColor();
        }

        protected virtual ConsoleColor GetMainNameColor(AssemblyReferenceInfo assemblyReferenceInfo)
        {
            return AssemblyLocalColor;
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
                    statusColor = AssemblyNotFoundColor;
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
            Console.WriteLine(AsmSpy_CommandLine.ConsoleVisualizer_VisualizeAssemblyReferenceInfo____0_, assemblyReferenceInfo.AssemblyName);
            Console.Write(AsmSpy_CommandLine.Source_, assemblyReferenceInfo.AssemblySource);
            if (assemblyReferenceInfo.AssemblySource != AssemblySource.NotFound)
            {
                Console.WriteLine(AsmSpy_CommandLine.Location_, assemblyReferenceInfo.ReflectionOnlyAssembly.Location);
            }
            else
            {
                Console.WriteLine();
            }
        }

        CommandOption unreferencedAssembliesOption;

        public void CreateOption(CommandLineApplication commandLineApplication)
        {
            unreferencedAssembliesOption = commandLineApplication.Option("-ua|--unref", "Show unreferenced assembly files on console.", CommandOptionType.NoValue);
        }

        public bool IsConfigured() => unreferencedAssembliesOption.HasValue();
    }
}

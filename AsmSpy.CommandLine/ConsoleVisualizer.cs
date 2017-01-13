using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace AsmSpy.CommandLine
{
    public class ConsoleVisualizer : IDependencyVisualizer
    {
        private const ConsoleColor AssemblyNotFoundColor = ConsoleColor.Red;
        private const ConsoleColor AssemblyLocalColor = ConsoleColor.Green;
        private const ConsoleColor AssemblyGlobalAssemblyCacheColor = ConsoleColor.Yellow;
        private const ConsoleColor AssemblyUnknownColor = ConsoleColor.Magenta;

        #region Fields

        private readonly DependencyAnalyzerResult _analyzerResult;

        #endregion

        #region Properties

        public bool OnlyConflicts { get; set; }
        public bool SkipSystem { get; set; }

        public string ReferencedStartsWith { get; set; }

        #endregion

        #region Constructor

        public ConsoleVisualizer(DependencyAnalyzerResult result)
        {
            _analyzerResult = result;
        }

        #endregion

        #region Visualize Support

        public virtual void Visualize()
        {
            if (_analyzerResult.AnalyzedFiles.Count <= 0)
            {
                Console.WriteLine("No assemblies files found in directory");
                return;
            }

            if (OnlyConflicts)
            {
                Console.WriteLine("Detailing only conflicting assembly references.");
            }

            var assemblyGroups = _analyzerResult.Assemblies.Values.GroupBy(x => x.AssemblyName.Name);

            foreach (var assemblyGroup in assemblyGroups.OrderBy(i => i.Key))
            {
                if (SkipSystem && (assemblyGroup.Key.ToUpperInvariant().StartsWith("SYSTEM", StringComparison.OrdinalIgnoreCase) || assemblyGroup.Key.ToUpperInvariant().StartsWith("MSCORLIB", StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                var assemblyInfos = assemblyGroup.OrderBy(x => x.AssemblyName.ToString()).ToList();
                if (OnlyConflicts && assemblyInfos.Count <= 1)
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(ReferencedStartsWith) && !assemblyInfos.SelectMany(x => x.ReferencedBy).GroupBy(x => x.AssemblyName.Name.ToUpperInvariant().StartsWith(ReferencedStartsWith.ToUpperInvariant(), StringComparison.OrdinalIgnoreCase)).Any())
                {
                    continue;
                }

                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("Reference: ");
                    Console.ForegroundColor = GetMainNameColor(assemblyInfos);
                Console.WriteLine("{0}", assemblyGroup.Key);
                
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
                mainNameColor = AssemblyLocalColor;
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
                    throw new InvalidEnumArgumentException("Invalid AssemblySource");
            }
            Console.ForegroundColor = statusColor;
            Console.WriteLine("  {0}", assemblyReferenceInfo.AssemblyName);
            Console.Write("  Source: {0}", assemblyReferenceInfo.AssemblySource);
            if (assemblyReferenceInfo.AssemblySource != AssemblySource.NotFound)
            {
                Console.WriteLine(", Location: {0}", assemblyReferenceInfo.ReflectionOnlyAssembly.Location);
            }
            else
            {
                Console.WriteLine();
            }

            foreach (var referer in assemblyReferenceInfo.ReferencedBy.OrderBy(x => x.AssemblyName.ToString()))
            {
                Console.ForegroundColor = statusColor;
                Console.Write("    {0}", assemblyReferenceInfo.AssemblyName.Version);

                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(" by ");

                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("{0}", referer.AssemblyName);
            }
        }

        #endregion
    }
}

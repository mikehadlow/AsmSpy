using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsmSpy
{
    public class ConsoleVisualizer : IDependencyVisualizer
    {
        private const ConsoleColor AssemblyNotFoundColor = ConsoleColor.Red;
        private const ConsoleColor AssemblyLocalColor = ConsoleColor.Green;
        private const ConsoleColor AssemblyGACColor = ConsoleColor.Yellow;
        private const ConsoleColor AssemblyUnknownColor = ConsoleColor.Magenta;

        #region Fields

        DependencyAnalyzerResult _AnalyzerResult;

        #endregion

        #region Properties

        public bool OnlyConflicts { get; set; }
        public bool SkipSystem { get; set; }

        #endregion

        #region Constructor

        public ConsoleVisualizer(DependencyAnalyzerResult result)
        {
            _AnalyzerResult = result;
        }

        #endregion

        #region Visualize Support

        public void Visualize()
        {
            if (_AnalyzerResult.AnalyzedFiles.Length <= 0)
            {
                Console.WriteLine("No assemblies files found in directory");
                return;
            }

            if (OnlyConflicts)
            {
                Console.WriteLine("Detailing only conflicting assembly references.");
            }

            var assemblyGroups = _AnalyzerResult.Assemblies.Values.GroupBy(x => x.AssemblyName.Name);

            foreach (var assemblyGroup in assemblyGroups.OrderBy(i => i.Key))
            {
                if (SkipSystem && (assemblyGroup.Key.StartsWith("System") || assemblyGroup.Key.StartsWith("mscorlib"))) continue;

                var assemblyInfos = assemblyGroup.OrderBy(x => x.AssemblyName.ToString()).ToList();
                if (OnlyConflicts && assemblyInfos.Count <= 1) continue;

                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("Reference: ");

                ConsoleColor mainNameColor;
                if (assemblyInfos.Any(x => x.AssemblySource == AssemblySource.Unknown))
                {
                    mainNameColor = AssemblyUnknownColor;
                }
                else if (assemblyInfos.Any(x => x.AssemblySource == AssemblySource.NotFound))
                {
                    mainNameColor = AssemblyNotFoundColor;
                }
                else if (assemblyInfos.Any(x => x.AssemblySource == AssemblySource.GAC))
                {
                    mainNameColor = AssemblyGACColor;
                }
                else 
                {
                    mainNameColor = AssemblyLocalColor;
                }

                Console.ForegroundColor = mainNameColor;
                Console.WriteLine("{0}", assemblyGroup.Key);
                
                for (var i = 0; i < assemblyInfos.Count; i++)
                {
                    var assemblyInfo = assemblyInfos[i];

                    ConsoleColor statusColor;
                    switch (assemblyInfo.AssemblySource)
                    {
                        case AssemblySource.NotFound:
                            statusColor = AssemblyNotFoundColor;
                            break;
                        case AssemblySource.Local:
                            statusColor = AssemblyLocalColor;
                            break;
                        case AssemblySource.GAC:
                            statusColor = AssemblyGACColor;
                            break;
                        case AssemblySource.Unknown:
                            statusColor = AssemblyUnknownColor;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    Console.ForegroundColor = statusColor;
                    Console.WriteLine("  {0}", assemblyInfo.AssemblyName);
                    Console.Write("  Source: {0}", assemblyInfo.AssemblySource);
                    if (assemblyInfo.AssemblySource != AssemblySource.NotFound)
                    {
                        Console.WriteLine(", Location: {0}", assemblyInfo.ReflectionOnlyAssembly.Location);
                    }
                    else
                    {
                        Console.WriteLine();
                    }

                    foreach (var referer in assemblyInfo.ReferencedBy.OrderBy(x => x.AssemblyName.ToString()))
                    {
                        Console.ForegroundColor = statusColor;
                        Console.Write("    {0}", assemblyInfo.AssemblyName.Version);

                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write(" by ");

                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.WriteLine("{0}", referer.AssemblyName);
                    }
                }

                Console.WriteLine();
            }
            Console.ResetColor();
        }

        #endregion
    }
}

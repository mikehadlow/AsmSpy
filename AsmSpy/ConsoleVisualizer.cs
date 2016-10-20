using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsmSpy
{
    public class ConsoleVisualizer : IDependencyVisualizer
    {
        static readonly ConsoleColor[] ConsoleColors = new ConsoleColor[]
        {
            ConsoleColor.Green,
            ConsoleColor.Red,
            ConsoleColor.Yellow,
            ConsoleColor.Blue,
            ConsoleColor.Cyan,
            ConsoleColor.Magenta,
        };

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
                Console.WriteLine("No dll files found in directory");
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

                var totalReferenceCount = assemblyGroup.Sum(x => x.ReferencedBy.Length);

                if (!OnlyConflicts || totalReferenceCount > 0)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("Reference: ");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine("{0}", assemblyGroup.Key);

                    //var assemblyInfos = assemblyGroup.OrderBy(x => x.AssemblyName.Version).ToList();
                    var assemblyInfos = assemblyGroup.OrderByDescending(x => x.ReferencedBy.Length).ToList();
                    for (var i = 0; i < assemblyInfos.Count; i++)
                    {
                        var assemblyInfo = assemblyInfos[i];
                        var versionColor = ConsoleColors[i % ConsoleColors.Length];

                        Console.ForegroundColor = versionColor;
                        Console.WriteLine("  {0}", assemblyInfo.AssemblyName);

                        foreach (var referer in assemblyInfo.ReferencedBy)
                        {
                            Console.ForegroundColor = versionColor;
                            Console.Write("    {0}", assemblyInfo.AssemblyName.Version);

                            Console.ForegroundColor = ConsoleColor.White;
                            Console.Write(" by ");

                            Console.ForegroundColor = ConsoleColor.Gray;
                            Console.WriteLine("{0}", referer.AssemblyName);
                        }
                    }

                    Console.WriteLine();
                }
            }
        }

        #endregion
    }
}

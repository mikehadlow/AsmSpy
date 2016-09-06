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

            foreach (var assemblyReferences in _AnalyzerResult.Assemblies.OrderBy(i => i.Key))
            {
                if (SkipSystem && (assemblyReferences.Key.StartsWith("System") || assemblyReferences.Key.StartsWith("mscorlib"))) continue;

                var referencesTo = assemblyReferences.Value.ReferencedBy;

                if (!OnlyConflicts || referencesTo.Length > 0)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("Reference: ");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine("{0}", assemblyReferences.Key);

                    var referencedAssemblies = new List<Tuple<string, string>>();
                    var versionsList = new List<string>();
                    var asmList = new List<string>();
                    foreach (var referencedAssembly in referencesTo)
                    {
                        var s1 = referencedAssembly.AssemblyName.Version.ToString();
                        var s2 = referencedAssembly.AssemblyName.FullName;
                        var tuple = new Tuple<string, string>(s1, s2);
                        referencedAssemblies.Add(tuple);
                    }

                    foreach (var referencedAssembly in referencedAssemblies)
                    {
                        if (!versionsList.Contains(referencedAssembly.Item1))
                        {
                            versionsList.Add(referencedAssembly.Item1);
                        }
                        if (!asmList.Contains(referencedAssembly.Item1))
                        {
                            asmList.Add(referencedAssembly.Item1);
                        }
                    }

                    foreach (var referencedAssembly in referencedAssemblies)
                    {
                        var versionColor = ConsoleColors[versionsList.IndexOf(referencedAssembly.Item1) % ConsoleColors.Length];

                        Console.ForegroundColor = versionColor;
                        Console.Write("   {0}", referencedAssembly.Item1);

                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write(" by ");

                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.WriteLine("{0}", referencedAssembly.Item2);
                    }

                    Console.WriteLine();
                }
            }
        }

        #endregion
    }
}

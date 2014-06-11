using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using AsmSpy.Native;

namespace AsmSpy
{
    public class Program
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
        static void Main(string[] args)
        {
            if (args.Length > 3 || args.Length < 1)
            {
                PrintUsage();
                return;
            }

            var directoryPath = args[0];
            if (!Directory.Exists(directoryPath))
            {
                PrintDirectoryNotFound(directoryPath);
                return;
            }


            var onlyConflicts = !args.Skip(1).Any(x => x.Equals("all", StringComparison.OrdinalIgnoreCase));  // args.Length != 2 || (args[1] != "all");
            var skipSystem = args.Skip(1).Any(x => x.Equals("nonsystem", StringComparison.OrdinalIgnoreCase));

            AnalyseAssemblies(new DirectoryInfo(directoryPath), onlyConflicts, skipSystem);
        }

        public static void AnalyseAssemblies(DirectoryInfo directoryInfo, bool onlyConflicts, bool skipSystem)
        {
            var assemblyFiles = directoryInfo.GetFiles("*.dll").Concat(directoryInfo.GetFiles("*.exe"));
            if (!assemblyFiles.Any())
            {
                Console.WriteLine("No dll files found in directory: '{0}'",
                    directoryInfo.FullName);
                return;
            }

            Console.WriteLine("Check assemblies in:");
            Console.WriteLine(directoryInfo.FullName);
            Console.WriteLine("");

            var assemblies = new Dictionary<string, IList<ReferencedAssembly>>();
            foreach (var fileInfo in assemblyFiles.OrderBy(asm => asm.Name))
            {
                Assembly assembly = null;
                try
                {
                    if (!fileInfo.IsAssembly())
                    {
                        continue;
                    }
                    assembly = Assembly.ReflectionOnlyLoadFrom(fileInfo.FullName);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to load assembly '{0}': {1}", fileInfo.FullName, ex.Message);
                    continue;
                }

                foreach (var referencedAssembly in assembly.GetReferencedAssemblies())
                {
                    if (!assemblies.ContainsKey(referencedAssembly.Name))
                    {
                        assemblies.Add(referencedAssembly.Name, new List<ReferencedAssembly>());
                    }
                    assemblies[referencedAssembly.Name]
                        .Add(new ReferencedAssembly(referencedAssembly.Version, assembly));
                }
            }

            if (onlyConflicts)
                Console.WriteLine("Detailing only conflicting assembly references.");

            foreach (var assembly in assemblies)
            {
                if (skipSystem && (assembly.Key.StartsWith("System") || assembly.Key.StartsWith("mscorlib"))) continue;
                
                if (!onlyConflicts
                    || (onlyConflicts && assembly.Value.GroupBy(x => x.VersionReferenced).Count() != 1))
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("Reference: ");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine("{0}", assembly.Key);

                    var referencedAssemblies = new List<Tuple<string, string>>();
                    var versionsList = new List<string>();
                    var asmList = new List<string>();
                    foreach (var referencedAssembly in assembly.Value)
                    {
                        var s1 = referencedAssembly.VersionReferenced.ToString();
                        var s2 = referencedAssembly.ReferencedBy.GetName().Name;
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
                        var versionColor = ConsoleColors[versionsList.IndexOf(referencedAssembly.Item1)%ConsoleColors.Length];

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

        private static void PrintDirectoryNotFound(string directoryPath)
        {
            Console.WriteLine("Directory: '" + directoryPath + "' does not exist.");
        }

        private static void PrintUsage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("AsmSpy <directory to load assemblies from> [all]");
            Console.WriteLine("E.g.");
            Console.WriteLine(@"AsmSpy C:\Source\My.Solution\My.Project\bin\Debug");
            Console.WriteLine(@"AsmSpy C:\Source\My.Solution\My.Project\bin\Debug all");
        }
    }

    public class ReferencedAssembly
    {
        public Version VersionReferenced { get; private set; }
        public Assembly ReferencedBy { get; private set; }

        public ReferencedAssembly(Version versionReferenced, Assembly referencedBy)
        {
            VersionReferenced = versionReferenced;
            ReferencedBy = referencedBy;
        }
    }
}

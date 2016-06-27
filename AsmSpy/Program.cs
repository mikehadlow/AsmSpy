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

        static readonly string[] HelpSwitches = new string[] { "/?", "-?", "-help", "--help" };
        static readonly string[] NonSystemSwitches = new string[] { "/n", "nonsystem", "/nonsystem" };
        static readonly string[] AllSwitches = new string[] { "/a", "all", "/all" };

        static void Main(string[] args)
        {
            if (
                args.Length > 3 || 
                args.Length < 1 || 
                args.Any(a => HelpSwitches.Contains(a, StringComparer.OrdinalIgnoreCase)))
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


            var onlyConflicts = !args.Skip(1).Any(x => AllSwitches.Contains(x, StringComparer.OrdinalIgnoreCase));  
            var skipSystem = args.Skip(1).Any(x => NonSystemSwitches.Contains(x, StringComparer.OrdinalIgnoreCase));

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

            var assemblies = new Dictionary<string, IList<ReferencedAssembly>>(StringComparer.OrdinalIgnoreCase);
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

            foreach (var assemblyReferences in assemblies.OrderBy(i => i.Key))
            {
                if (skipSystem && (assemblyReferences.Key.StartsWith("System") || assemblyReferences.Key.StartsWith("mscorlib"))) continue;

                if (!onlyConflicts || assemblyReferences.Value.GroupBy(x => x.VersionReferenced).Count() != 1)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("Reference: ");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine("{0}", assemblyReferences.Key);

                    var referencedAssemblies = new List<Tuple<string, string>>();
                    var versionsList = new List<string>();
                    var asmList = new List<string>();
                    foreach (var referencedAssembly in assemblyReferences.Value)
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

        private static void PrintDirectoryNotFound(string directoryPath)
        {
            Console.WriteLine("Directory: '" + directoryPath + "' does not exist.");
        }

        private static void PrintUsage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("AsmSpy <directory to load assemblies from> [all|nonsystem]");
            Console.WriteLine("E.g.");
            Console.WriteLine(@"AsmSpy C:\Source\My.Solution\My.Project\bin\Debug");
            Console.WriteLine(@"AsmSpy C:\Source\My.Solution\My.Project\bin\Debug all");
            Console.WriteLine(@"AsmSpy C:\Source\My.Solution\My.Project\bin\Debug nonsystem");
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

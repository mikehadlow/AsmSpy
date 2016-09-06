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

            IDependencyAnalyzer analyzer = new DependencyAnalyzer() { DirectoryInfo = new DirectoryInfo(directoryPath) };

            Console.WriteLine("Check assemblies in:");
            Console.WriteLine(analyzer.DirectoryInfo);
            Console.WriteLine("");

            var result = analyzer.Analyze(assemblyName => Console.WriteLine(string.Format("Checking {0}", assemblyName)));

            IDependencyVisualizer visualizer = new ConsoleVisualizer(result) { SkipSystem = skipSystem, OnlyConflicts = onlyConflicts };
            visualizer.Visualize();

            Console.ReadLine();

        }

        private static void PrintDirectoryNotFound(string directoryPath)
        {
            Console.WriteLine("Directory: '" + directoryPath + "' does not exist.");
        }

        private static void PrintUsage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("AsmSpy <directory to load assemblies from> [options]");
            Console.WriteLine();

            Console.WriteLine("Switches:");
            Console.WriteLine("/all       : list all assemblies and references. Supported formats:  " + string.Join(",", AllSwitches));
            Console.WriteLine("/nonsystem : list system assemblies. Supported formats:  " + string.Join(",", NonSystemSwitches));
            Console.WriteLine();

            Console.WriteLine("E.g.");
            Console.WriteLine(@"AsmSpy C:\Source\My.Solution\My.Project\bin\Debug");
            Console.WriteLine(@"AsmSpy C:\Source\My.Solution\My.Project\bin\Debug all");
            Console.WriteLine(@"AsmSpy C:\Source\My.Solution\My.Project\bin\Debug nonsystem");
        }
    }
}

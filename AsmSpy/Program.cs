using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using AsmSpy.Native;
using CommandLine;
using CommandLine.Text;

namespace AsmSpy
{
    public class Program
    {
        public class Options
        {
            [ValueList(typeof(List<string>), MaximumElements = 1)]
            public IList<string> DirectoryPath { get; set; }

            [Option('n', "nonsystem", HelpText = "list system assemblies")]
            public bool SkipSystem { get; set; }

            [Option("dgml", HelpText = "export the references to DGML")]
            public bool ExportToDgml { get; set; }

            [Option("output", HelpText = "export filename")]
            public string Output { get; set; }

            [HelpOption]
            public string GetUsage()
            {
                var help = new HelpText
                {
                    AdditionalNewLineAfterOption = true,
                    AddDashesToOption = true,
                };
                help.AddPreOptionsLine("AsmSpy <directory to load assemblies from> [options]");
                help.AddOptions(this);
                help.AddPostOptionsLine(@"AsmSpy C:\Source\My.Solution\My.Project\bin\Debug");
                help.AddPostOptionsLine(@"AsmSpy C:\Source\My.Solution\My.Project\bin\Debug all");
                help.AddPostOptionsLine(@"AsmSpy C:\Source\My.Solution\My.Project\bin\Debug nonsystem");
                return help;
            }
        }

        static readonly string[] HelpSwitches = new string[] { "/?", "-?", "-help", "--help" };
        static readonly string[] NonSystemSwitches = new string[] { "/n", "nonsystem", "/nonsystem" };
        static readonly string[] AllSwitches = new string[] { "/a", "all", "/all" };

        static void Main(string[] args)
        {
            Options options = new Options();
             
            if (!Parser.Default.ParseArguments(args, options) || options.DirectoryPath.Count == 0)
            {
                return;
            }

            var directoryPath = options.DirectoryPath[0];
            if (!Directory.Exists(directoryPath))
            {
                PrintDirectoryNotFound(directoryPath);
                return;
            }

            var onlyConflicts = !args.Skip(1).Any(x => AllSwitches.Contains(x, StringComparer.OrdinalIgnoreCase));
            var skipSystem = options.SkipSystem; 

            IDependencyAnalyzer analyzer = new DependencyAnalyzer() { DirectoryInfo = new DirectoryInfo(directoryPath) };

            Console.WriteLine("Check assemblies in:");
            Console.WriteLine(analyzer.DirectoryInfo);
            Console.WriteLine("");
            var result = analyzer.Analyze(assemblyName => Console.WriteLine(string.Format("Checking {0}", assemblyName)));

            IDependencyVisualizer visualizer = new ConsoleVisualizer(result) { SkipSystem = skipSystem, OnlyConflicts = onlyConflicts };
            visualizer.Visualize();

            if (options.ExportToDgml)
            {
                IDependencyVisualizer dgmlExport = new DgmlExport(result, string.IsNullOrWhiteSpace(options.Output) ? Path.Combine(analyzer.DirectoryInfo.FullName, "references.dgml") : options.Output, new ConsoleLogger());
                dgmlExport.Visualize();
            }
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

using System;
using System.IO;
using Microsoft.Extensions.CommandLineUtils;

namespace AsmSpy
{
    public class Program
    {
        static void Main(string[] args)
        {
            CommandLineApplication commandLineApplication = new CommandLineApplication(throwOnUnexpectedArg: false);
            CommandArgument directory = commandLineApplication.Argument("directory", "The directory to search for assemblies");
            CommandOption dgmlExport = commandLineApplication.Option("-dg|--dgml <filename>", "Export to a dgml file", CommandOptionType.SingleValue);
            CommandOption nonsystem = commandLineApplication.Option("-n|--nonsystem", "List system assemblies", CommandOptionType.NoValue);
            CommandOption all = commandLineApplication.Option("-a|--all", "List all assemblies and references.", CommandOptionType.NoValue);
            CommandOption noconsole = commandLineApplication.Option("-nc|--noconsole", "Do not show references on console.", CommandOptionType.NoValue);
            CommandOption silent = commandLineApplication.Option("-s|--silent", "Do not show any message, only warnings and errors will be shown.", CommandOptionType.NoValue);

            commandLineApplication.HelpOption("-? | -h | --help");
            commandLineApplication.OnExecute(() =>
            {
                var consoleLogger = new ConsoleLogger(!silent.HasValue());

                var directoryPath = directory.Value;
                if (!Directory.Exists(directoryPath))
                {
                    consoleLogger.LogMessage(string.Format("Directory: '{0}' does not exist.", directoryPath));
                    return -1;
                }

                var onlyConflicts = !all.HasValue();
                var skipSystem = nonsystem.HasValue();

                IDependencyAnalyzer analyzer = new DependencyAnalyzer() { DirectoryInfo = new DirectoryInfo(directoryPath) };

                consoleLogger.LogMessage(string.Format("Check assemblies in: {0}", analyzer.DirectoryInfo));

                var result = analyzer.Analyze(consoleLogger);

                if (!noconsole.HasValue())
                {
                    IDependencyVisualizer visualizer = new ConsoleVisualizer(result) { SkipSystem = skipSystem, OnlyConflicts = onlyConflicts };
                    visualizer.Visualize();
                }

                if (dgmlExport.HasValue())
                {
                    IDependencyVisualizer export = new DgmlExport(result, string.IsNullOrWhiteSpace(dgmlExport.Value()) ? Path.Combine(analyzer.DirectoryInfo.FullName, "references.dgml") : dgmlExport.Value(), consoleLogger);
                    export.Visualize();
                }

                return 0;
            });
            try
            {
                commandLineApplication.Execute(args);
            }
            catch (CommandParsingException cpe)
            {
                Console.WriteLine(cpe.Message);
                commandLineApplication.ShowHelp();
            }
        }
    }
}

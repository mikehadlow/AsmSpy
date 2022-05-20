using AsmSpy.CommandLine.Visualizers;
using AsmSpy.Core;

using Microsoft.Extensions.CommandLineUtils;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace AsmSpy.CommandLine
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            Console.OutputEncoding = Encoding.Unicode;

            var commandLineApplication = new CommandLineApplication(throwOnUnexpectedArg: true);
            var directoryOrFile = commandLineApplication.Argument("directoryOrFile", "The directory to search for assemblies or file path to a single assembly");

            var silent = commandLineApplication.Option("-s|--silent", "Do not show any message, only warnings and errors will be shown.", CommandOptionType.NoValue);

            var nonsystem = commandLineApplication.Option("-n|--nonsystem", "Ignore 'System' assemblies", CommandOptionType.NoValue);
            var all = commandLineApplication.Option("-a|--all", "List all assemblies and references.", CommandOptionType.NoValue);
            var referencedStartsWith = commandLineApplication.Option("-rsw|--referencedstartswith", "Referenced Assembly should start with <string>. Will only analyze assemblies if their referenced assemblies starts with the given value.", CommandOptionType.SingleValue);
            var excludeAssemblies = commandLineApplication.Option("-e|--exclude", "A partial assembly name which should be excluded. This option can be provided multiple times", CommandOptionType.MultipleValue);

            var includeSubDirectories = commandLineApplication.Option("-i|--includesub", "Include subdirectories in search", CommandOptionType.NoValue);
            var configurationFile = commandLineApplication.Option("-c|--configurationFile", "Use the binding redirects of the given configuration file (Web.config or App.config)", CommandOptionType.SingleValue);
            var failOnMissing = commandLineApplication.Option("-f|--failOnMissing", "Whether to exit with an error code when AsmSpy detected Assemblies which could not be found", CommandOptionType.NoValue);

            var dependencyVisualizers = GetDependencyVisualizers();
            foreach (var visualizer in dependencyVisualizers)
            {
                visualizer.CreateOption(commandLineApplication);
            }

            commandLineApplication.HelpOption("-? | -h | --help");

            commandLineApplication.OnExecute(() =>
            {
                try
                {
                    var visualizerOptions = new VisualizerOptions
                    {
                        SkipSystem = nonsystem.HasValue(),
                        OnlyConflicts = !all.HasValue(),
                        ReferencedStartsWith = referencedStartsWith.HasValue() ? referencedStartsWith.Value() : string.Empty,
                        Exclude = excludeAssemblies.HasValue() ? excludeAssemblies.Values : new List<string>()
                    };

                    var consoleLogger = new ConsoleLogger(!silent.HasValue());

                    var finalResult = GetFileList(directoryOrFile, includeSubDirectories, consoleLogger)
                        .Bind(x => GetAppDomainWithBindingRedirects(configurationFile)
                            .Map(appDomain => DependencyAnalyzer.Analyze(
                                x.FileList,
                                appDomain,
                                consoleLogger,
                                visualizerOptions,
                                x.RootFileName)))
                        .Map(result => RunVisualizers(result, consoleLogger, visualizerOptions))
                        .Bind(FailOnMissingAssemblies);

                    switch (finalResult)
                    {
                        case Failure<bool> fail:
                            consoleLogger.LogError(fail.Message);
                            return -1;
                        case Success<bool> succeed:
                            return 0;
                        default:
                            throw new InvalidOperationException("Unexpected result type");
                    }

                    DependencyAnalyzerResult RunVisualizers(DependencyAnalyzerResult dependencyAnalyzerResult, ILogger logger, VisualizerOptions options)
                    {
                        foreach (var visualizer in dependencyVisualizers.Where(x => x.IsConfigured()))
                        {
                            visualizer.Visualize(dependencyAnalyzerResult, logger, options);
                        }
                        return dependencyAnalyzerResult;
                    }

                    Result<bool> FailOnMissingAssemblies(DependencyAnalyzerResult dependencyAnalyzerResult)
                        => failOnMissing.HasValue() && dependencyAnalyzerResult.MissingAssemblies.Any()
                            ? "Missing Assemblies"
                            : Result<bool>.Succeed(true);
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.ToString());
                    return -1;
                }
            });

            try
            {
                if (args == null || args.Length == 0)
                {
                    commandLineApplication.ShowHelp();
                    return 0;
                }

                return commandLineApplication.Execute(args);
            }
            catch (CommandParsingException cpe)
            {
                Console.WriteLine(cpe.Message);
                commandLineApplication.ShowHelp();
                return -1;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        private static Result<(List<FileInfo> FileList, string RootFileName)> GetFileList(CommandArgument directoryOrFile, CommandOption includeSubDirectories, ILogger logger)
        {
            var searchPattern = includeSubDirectories.HasValue() ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var directoryOrFilePath = directoryOrFile.Value;
            var directoryPath = directoryOrFilePath;

            if (!File.Exists(directoryOrFilePath) && !Directory.Exists(directoryOrFilePath))
            {
                return (string.Format(CultureInfo.InvariantCulture, "Directory or file: '{0}' does not exist.", directoryOrFilePath));
            }

            var rootFileName = "";
            if (File.Exists(directoryOrFilePath))
            {
                rootFileName = Path.GetFileName(directoryOrFilePath);
                logger.LogMessage($"Root assembly specified: '{rootFileName}'");
                directoryPath = Path.GetDirectoryName(directoryOrFilePath);
            }

            var directoryInfo = new DirectoryInfo(directoryPath);

            logger.LogMessage($"Checking for local assemblies in: '{directoryInfo}', {searchPattern}");

            var fileList = directoryInfo.GetFiles("*.dll", searchPattern).Concat(directoryInfo.GetFiles("*.exe", searchPattern)).ToList();

            return (fileList, rootFileName);
        }

        public static Result<AppDomain> GetAppDomainWithBindingRedirects(CommandOption configurationFile)
        {
            var configurationFilePath = configurationFile.Value();
            if (!string.IsNullOrEmpty(configurationFilePath) && !File.Exists(configurationFilePath))
            {
                return $"Directory or file: '{configurationFilePath}' does not exist.";
            }

            try
            {
                var domaininfo = new AppDomainSetup
                {
                    ConfigurationFile = configurationFilePath
                };
                return AppDomain.CreateDomain("AppDomainWithBindingRedirects", null, domaininfo);
            }
            catch (Exception ex)
            {
                return $"Failed creating AppDomain from configuration file with message {ex.Message}";
            }
        }

        private static IDependencyVisualizer[] GetDependencyVisualizers() => new IDependencyVisualizer[]
        {
            new ConsoleVisualizer(),
            new ConsoleTreeVisualizer(),
            new DgmlExport(),
            new XmlExport(),
            new DotExport(),
            new BindingRedirectExport()
        };
    }
}

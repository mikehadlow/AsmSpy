using AsmSpy.Core;

using Microsoft.Extensions.CommandLineUtils;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace AsmSpy.CommandLine
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var commandLineApplication = new CommandLineApplication(throwOnUnexpectedArg: true);
            var directoryOrFile = commandLineApplication.Argument("directoryOrFile", "The directory to search for assemblies or file path to a single assembly");
            var dgmlExport = commandLineApplication.Option("-dg|--dgml <filename>", "Export to a dgml file", CommandOptionType.SingleValue);
            var nonsystem = commandLineApplication.Option("-n|--nonsystem", "Ignore 'System' assemblies", CommandOptionType.NoValue);
            var all = commandLineApplication.Option("-a|--all", "List all assemblies and references.", CommandOptionType.NoValue);
            var noconsole = commandLineApplication.Option("-nc|--noconsole", "Do not show references on console.", CommandOptionType.NoValue);
            var silent = commandLineApplication.Option("-s|--silent", "Do not show any message, only warnings and errors will be shown.", CommandOptionType.NoValue);
            var bindingRedirect = commandLineApplication.Option("-b|--bindingredirect", "Create binding-redirects", CommandOptionType.NoValue);
            var referencedStartsWith = commandLineApplication.Option("-rsw|--referencedstartswith", "Referenced Assembly should start with <string>. Will only analyze assemblies if their referenced assemblies starts with the given value.", CommandOptionType.SingleValue);
            var includeSubDirectories = commandLineApplication.Option("-i|--includesub", "Include subdirectories in search", CommandOptionType.NoValue);
            var configurationFile = commandLineApplication.Option("-c|--configurationFile", "Use the binding redirects of the given configuration file (Web.config or App.config)", CommandOptionType.SingleValue);

            commandLineApplication.HelpOption("-? | -h | --help");
            commandLineApplication.OnExecute(() =>
            {
                var consoleLogger = new ConsoleLogger(!silent.HasValue());

                var directoryOrFilePath = directoryOrFile.Value;
                var directoryPath = directoryOrFile.Value;

                if (!File.Exists(directoryOrFilePath) && !Directory.Exists(directoryOrFilePath))
                {
                    consoleLogger.LogMessage(string.Format(CultureInfo.InvariantCulture, "Directory or file: '{0}' does not exist.", directoryOrFilePath));
                    return -1;
                }

                var configurationFilePath = configurationFile.Value();
                if (!string.IsNullOrEmpty(configurationFilePath) && !File.Exists(configurationFilePath))
                {
                    consoleLogger.LogMessage(string.Format(CultureInfo.InvariantCulture, "Directory or file: '{0}' does not exist.", configurationFilePath));
                    return -1;
                }

                var isFilePathProvided = false;
                var fileName = "";
                if (File.Exists(directoryOrFilePath))
                {
                    isFilePathProvided = true;
                    fileName = Path.GetFileName(directoryOrFilePath);
                    directoryPath = Path.GetDirectoryName(directoryOrFilePath);
                }

                var onlyConflicts = !all.HasValue();
                var skipSystem = nonsystem.HasValue();
                var searchPattern = includeSubDirectories.HasValue() ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

                var directoryInfo = new DirectoryInfo(directoryPath);

                List<FileInfo> fileList;
                if (isFilePathProvided)
                {
                    fileList = directoryInfo.GetFiles(fileName, SearchOption.TopDirectoryOnly).ToList();
                    consoleLogger.LogMessage(string.Format(CultureInfo.InvariantCulture, "Check assemblies referenced in: {0}", directoryOrFilePath));
                }
                else
                {
                    fileList = directoryInfo.GetFiles("*.dll", searchPattern).Concat(directoryInfo.GetFiles("*.exe", searchPattern)).ToList();
                    consoleLogger.LogMessage(string.Format(CultureInfo.InvariantCulture, "Check assemblies in: {0}", directoryInfo));
                }

                AppDomain appDomainWithBindingRedirects = null;
                try
                {
                    var domaininfo = new AppDomainSetup
                    {
                        ConfigurationFile = configurationFilePath
                    };
                    appDomainWithBindingRedirects = AppDomain.CreateDomain("AppDomainWithBindingRedirects", null, domaininfo);
                }
                catch (Exception ex)
                {
                    consoleLogger.LogError($"Failed creating AppDomain from configuration file with message {ex.Message}");
                    return -1;
                }
                
                IDependencyAnalyzer analyzer = new DependencyAnalyzer(fileList, appDomainWithBindingRedirects);

                var result = analyzer.Analyze(consoleLogger);

                if (!noconsole.HasValue())
                {
                    IDependencyVisualizer visualizer = new ConsoleVisualizer(result) { SkipSystem = skipSystem, OnlyConflicts = onlyConflicts, ReferencedStartsWith = referencedStartsWith.HasValue() ? referencedStartsWith.Value() : string.Empty };
                    visualizer.Visualize();
                }

                if (dgmlExport.HasValue())
                {
                    IDependencyVisualizer export = new DgmlExport(result, string.IsNullOrWhiteSpace(dgmlExport.Value()) ? Path.Combine(directoryInfo.FullName, "references.dgml") : dgmlExport.Value(), consoleLogger) { SkipSystem = skipSystem };
                    export.Visualize();
                }

                if (bindingRedirect.HasValue())
                {
                    // binding redirect export explicitly doesn't respect SkipSystem
                    IDependencyVisualizer bindingRedirects = new BindingRedirectExport(result, string.IsNullOrWhiteSpace(dgmlExport.Value()) ? Path.Combine(directoryInfo.FullName, "bindingRedirects.xml") : dgmlExport.Value(), consoleLogger);
                    bindingRedirects.Visualize();
                }

                return 0;
            });
            try
            {
                if (args == null || args.Length == 0)
                {
                    commandLineApplication.ShowHelp();
                }
                else
                {
                    commandLineApplication.Execute(args);
                }
            }
            catch (CommandParsingException cpe)
            {
                Console.WriteLine(cpe.Message);
                commandLineApplication.ShowHelp();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace AsmSpy
{
    public class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
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

            AnalyseAssemblies(new DirectoryInfo(directoryPath));
        }

        public static void AnalyseAssemblies(DirectoryInfo directoryInfo)
        {
            var assemblyFiles = directoryInfo.GetFiles("*.dll");
            if (assemblyFiles.Length == 0)
            {
                Console.WriteLine("No dll files found in directory: '{0}'", 
                    directoryInfo.FullName);
                return;
            }

            Console.WriteLine("Check assemblies in:");
            Console.WriteLine(directoryInfo.FullName);
            Console.WriteLine("");

            var assemblies = new Dictionary<string, IList<ReferencedAssembly>>();
            foreach (var fileInfo in assemblyFiles)
            {
                Assembly assembly = null;
                try
                {
                    assembly = Assembly.LoadFrom(fileInfo.FullName);
                }
                catch (Exception)
                {
                    Console.WriteLine("Failed to load assembly: '{0}'", fileInfo.FullName);
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

            foreach (var assembly in assemblies)
            {
                Console.WriteLine("Reference: {0}", assembly.Key);
                foreach (var referencedAssembly in assembly.Value)
                {
                    Console.WriteLine("\t{0} by {1}", referencedAssembly.VersionReferenced, referencedAssembly.ReferencedBy.GetName().Name);
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
            Console.WriteLine("AsmSpy <directory to load assemblies from>");
            Console.WriteLine("E.g.");
            Console.WriteLine(@"AsmSpy C:\Source\My.Solution\My.Project\bin\Debug");
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

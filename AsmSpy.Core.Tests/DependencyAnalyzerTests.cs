using System;
using System.Linq;
using System.Reflection;
using System.IO;
using Xunit;
using Xunit.Abstractions;
using AsmSpy.Core.TestLibrary;
using System.Collections.Generic;

namespace AsmSpy.Core.Tests
{
    public class DependencyAnalyzerTests
    {
        private readonly ITestOutputHelper output;
        private readonly TestLogger logger;

        private IEnumerable<FileInfo> filesToAnalyse;
        private VisualizerOptions options = new VisualizerOptions(false, false, "");

        public DependencyAnalyzerTests(ITestOutputHelper output)
        {
            this.output = output ?? throw new ArgumentNullException(nameof(output));
            logger = new TestLogger(output);

            var thisAssembly = Assembly.GetExecutingAssembly();
            var testBinDirectory = Path.GetDirectoryName(thisAssembly.Location);
            output.WriteLine(testBinDirectory);

            filesToAnalyse = Directory.GetFiles(testBinDirectory, "*.dll").Select(x => new FileInfo(x));
        }

        [Fact]
        public void AnalyzeShouldReturnTestAssemblies()
        {
            var result = DependencyAnalyzer.Analyze(filesToAnalyse, null, logger, options);

            Assert.Contains(result.Assemblies.Values, x => x.AssemblyName.Name == "AsmSpy.Core");
            Assert.Contains(result.Assemblies.Values, x => x.AssemblyName.Name == "AsmSpy.Core.Tests");
            Assert.Contains(result.Assemblies.Values, x => x.AssemblyName.Name == "AsmSpy.Core.TestLibrary");
            Assert.Contains(result.Assemblies.Values, x => x.AssemblyName.Name == "xunit.core");
        }

        [Fact(Skip ="Fails in AppVeyor")]
        public void AnalyzeShouldReturnSystemAssemblies()
        {
            var result = DependencyAnalyzer.Analyze(filesToAnalyse, null, logger, options);

            Assert.Contains(result.Assemblies.Values, x => x.AssemblyName.Name == "mscorlib");
            Assert.Contains(result.Assemblies.Values, x => x.AssemblyName.Name == "netstandard");
            Assert.Contains(result.Assemblies.Values, x => x.AssemblyName.Name == "System");
        }

        [Fact]
        public void AnalyzeShouldNotReturnSystemAssembliesWhenFlagIsSet()
        {
            var altOptions = new VisualizerOptions(true, false, "");
            var result = DependencyAnalyzer.Analyze(filesToAnalyse, null, logger, altOptions);

            Assert.DoesNotContain(result.Assemblies.Values, x => x.AssemblyName.Name == "mscorlib");
            Assert.DoesNotContain(result.Assemblies.Values, x => x.AssemblyName.Name == "netstandard");
            Assert.DoesNotContain(result.Assemblies.Values, x => x.AssemblyName.Name == "System");
        }

        [Fact]
        public void AnalyzeShouldReturnDependencies()
        {
            var exampleClass = new ExampleClass();
            var result = DependencyAnalyzer.Analyze(filesToAnalyse, null, logger, options);

            var tests = result.Assemblies.Values.Single(x => x.AssemblyName.Name == "AsmSpy.Core.Tests");

            Assert.Contains(tests.References, x => x.AssemblyName.Name == "AsmSpy.Core");
            Assert.Contains(tests.References, x => x.AssemblyName.Name == "AsmSpy.Core.TestLibrary");
            Assert.Contains(tests.References, x => x.AssemblyName.Name == "xunit.core");
            foreach(var reference in tests.References)
            {
                output.WriteLine(reference.AssemblyName.Name);
            }
        }

        [Fact]
        public void AnalyzeShouldReturnCorrectAssemblySource()
        {
            var result = DependencyAnalyzer.Analyze(filesToAnalyse, null, logger, options);

            var tests = result.Assemblies.Values.Single(x => x.AssemblyName.Name == "AsmSpy.Core.Tests");

            var mscorlib = tests.References.Single(x => x.AssemblyName.Name == "mscorlib");
            Assert.Equal(AssemblySource.GlobalAssemblyCache, mscorlib.AssemblySource);
        }
    }
}

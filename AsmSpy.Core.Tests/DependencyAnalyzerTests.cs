using System;
using System.Linq;
using System.Reflection;
using System.IO;
using Xunit;
using Xunit.Abstractions;
using AsmSpy.Core.TestLibrary;

namespace AsmSpy.Core.Tests
{
    public class DependencyAnalyzerTests
    {
        private readonly ITestOutputHelper output;
        private readonly DependencyAnalyzer sut;
        private readonly TestLogger logger;

        public DependencyAnalyzerTests(ITestOutputHelper output)
        {
            this.output = output ?? throw new ArgumentNullException(nameof(output));
            logger = new TestLogger(output);

            var thisAssembly = Assembly.GetExecutingAssembly();
            var testBinDirectory = Path.GetDirectoryName(thisAssembly.Location);
            output.WriteLine(testBinDirectory);

            var filesToAnalyse = Directory.GetFiles(testBinDirectory, "*.dll").Select(x => new FileInfo(x));
            sut = new DependencyAnalyzer(filesToAnalyse);
        }

        [Fact]
        public void AnalyzeShouldReturnTestAssemblies()
        {
            var result = sut.Analyze(logger);

            Assert.Contains(result.Assemblies.Values, x => x.AssemblyName.Name == "AsmSpy.Core");
            Assert.Contains(result.Assemblies.Values, x => x.AssemblyName.Name == "AsmSpy.Core.Tests");
            Assert.Contains(result.Assemblies.Values, x => x.AssemblyName.Name == "AsmSpy.Core.TestLibrary");
            Assert.Contains(result.Assemblies.Values, x => x.AssemblyName.Name == "xunit.core");
        }

        [Fact(Skip ="Fails in AppVeyor")]
        public void AnalyzeShouldReturnSystemAssemblies()
        {
            var result = sut.Analyze(logger);

            Assert.Contains(result.Assemblies.Values, x => x.AssemblyName.Name == "mscorlib");
            Assert.Contains(result.Assemblies.Values, x => x.AssemblyName.Name == "netstandard");
            Assert.Contains(result.Assemblies.Values, x => x.AssemblyName.Name == "System");
        }

        [Fact]
        public void AnalyzeShouldNotReturnSystemAssembliesWhenFlagIsSet()
        {
            sut.SkipSystem = true;
            var result = sut.Analyze(logger);

            Assert.DoesNotContain(result.Assemblies.Values, x => x.AssemblyName.Name == "mscorlib");
            Assert.DoesNotContain(result.Assemblies.Values, x => x.AssemblyName.Name == "netstandard");
            Assert.DoesNotContain(result.Assemblies.Values, x => x.AssemblyName.Name == "System");
        }

        [Fact]
        public void AnalyzeShouldReturnDependencies()
        {
            var exampleClass = new ExampleClass();
            var result = sut.Analyze(logger);

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
            var result = sut.Analyze(logger);

            var tests = result.Assemblies.Values.Single(x => x.AssemblyName.Name == "AsmSpy.Core.Tests");

            var mscorlib = tests.References.Single(x => x.AssemblyName.Name == "mscorlib");
            Assert.Equal(AssemblySource.GlobalAssemblyCache, mscorlib.AssemblySource);
        }
    }
}

using System.IO;

namespace AsmSpy
{
    public class UnitTests
    {
        public void AnalyseAssemblies_should_output_correct_info()
        {
            var path = @"D:\Source\sutekishop\Suteki.Shop\Suteki.Shop\bin";
            Program.AnalyseAssemblies(new DirectoryInfo(path));
        }
    }
}